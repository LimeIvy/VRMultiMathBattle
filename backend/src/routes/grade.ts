import { OpenAPIHono } from "@hono/zod-openapi";
import { drizzle } from "drizzle-orm/d1";
import { createRoute } from "@hono/zod-openapi";
import { z } from "@hono/zod-openapi";
import { eq } from "drizzle-orm";
import { GradeRequestSchema, GradeResponseSchema } from "../types/grade";
import { problemsTable, gradesTable } from "../database/schema";

type Bindings = {
  DB: D1Database;
  GEMINI_API_KEY: string;
};

// Gemini APIのレスポンス型
interface GeminiResponse {
  candidates?: Array<{
    content?: {
      parts?: Array<{
        text?: string;
      }>;
    };
  }>;
}

const gradeApp = new OpenAPIHono<{ Bindings: Bindings }>();

// POST /grade エンドポイント: 画像を送信して採点
const gradeRoute = createRoute({
  method: "post",
  path: "/grade",
  tags: ["Grade"],
  summary: "画像を送信して採点",
  description: "黒板の画像をAIに送信し、解答を採点します",
  request: {
    body: {
      content: {
        "application/json": {
          schema: GradeRequestSchema,
        },
      },
    },
  },
  responses: {
    200: {
      content: {
        "application/json": {
          schema: GradeResponseSchema,
        },
      },
      description: "採点結果",
    },
    404: {
      content: {
        "application/json": {
          schema: z.object({
            error: z.string(),
          }),
        },
      },
      description: "問題が見つかりません",
    },
    500: {
      content: {
        "application/json": {
          schema: z.object({
            error: z.string(),
            details: z.string().optional(),
          }),
        },
      },
      description: "サーバーエラー",
    },
  },
});

gradeApp.openapi(gradeRoute, async (c) => {
  try {
    const { problemId, imageBase64, playerName } = c.req.valid("json");
    const db = drizzle(c.env.DB);

    // 問題をデータベースから取得
    const problems = await db
      .select()
      .from(problemsTable)
      .where(eq(problemsTable.id, problemId))
      .all();

    if (problems.length === 0) {
      return c.json({ error: "問題が見つかりません" }, 404);
    }

    const problem = problems[0];

    // Gemini APIに送信するプロンプトを構築
    const prompt = `あなたは数学の先生です。生徒が黒板に書いた解答を採点してください。

問題: ${problem.question}
正解: ${problem.correctAnswer}
難易度: ${problem.difficulty}
カテゴリ: ${problem.category}

以下の画像には生徒の解答が含まれています。解答を確認し、以下の形式でJSON形式で返してください：
{
  "isCorrect": true または false,
  "feedback": "フィードバックメッセージ"
}

フィードバックは日本語で、簡潔に（1-2文）で書いてください。`;

    // Gemini APIにリクエストを送信
    const geminiResponse = await fetch(
      `https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=${c.env.GEMINI_API_KEY}`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          contents: [
            {
              parts: [
                { text: prompt },
                {
                  inline_data: {
                    mime_type: "image/jpeg",
                    data: imageBase64,
                  },
                },
              ],
            },
          ],
          generationConfig: {
            temperature: 0.4,
            topK: 32,
            topP: 1,
            maxOutputTokens: 512,
          },
        }),
      }
    );

    if (!geminiResponse.ok) {
      const errorText = await geminiResponse.text();
      console.error("Gemini API error:", errorText);
      return c.json(
        {
          error: "AI採点に失敗しました",
          details: errorText,
        },
        500
      );
    }

    const geminiData = (await geminiResponse.json()) as GeminiResponse;
    console.log("Gemini response:", JSON.stringify(geminiData, null, 2));

    // Geminiのレスポンスからテキストを抽出
    const responseText =
      geminiData.candidates?.[0]?.content?.parts?.[0]?.text || "";

    // JSONレスポンスをパース
    let gradeResult;
    try {
      // JSONを抽出（```json ... ``` のマークダウン形式にも対応）
      const jsonMatch = responseText.match(/\{[\s\S]*\}/);
      if (jsonMatch) {
        gradeResult = JSON.parse(jsonMatch[0]);
      } else {
        throw new Error("JSON形式のレスポンスが見つかりません");
      }
    } catch (parseError) {
      console.error("JSON parse error:", parseError);
      console.error("Response text:", responseText);
      
      // フォールバック: テキストから情報を抽出
      const isCorrect = responseText.toLowerCase().includes("正解") || 
                       responseText.toLowerCase().includes("correct");
      gradeResult = {
        isCorrect,
        feedback: responseText.substring(0, 200), // 最初の200文字
      };
    }

    // 採点結果をデータベースに保存
    const savedGrade = await db
      .insert(gradesTable)
      .values({
        problemId,
        playerName: playerName || null,
        isCorrect: gradeResult.isCorrect,
        feedback: gradeResult.feedback,
      })
      .returning();

    return c.json(
      {
        id: savedGrade[0].id,
        isCorrect: gradeResult.isCorrect,
        feedback: gradeResult.feedback,
      },
      200
    );
  } catch (error) {
    console.error("Error in grade endpoint:", error);
    return c.json(
      {
        error: "採点処理中にエラーが発生しました",
        details: error instanceof Error ? error.message : String(error),
      },
      500
    );
  }
});

export default gradeApp;
