import { createInsertSchema } from "drizzle-zod";
import { problemsTable } from "../database/schema"; // Drizzle スキーマをインポート
import { z } from "@hono/zod-openapi"; // Zod OpenAPI をインポート

// 問題のレスポンススキーマ（OpenAPI用）
export const ProblemSchema = z
  .object({
    id: z.number().openapi({
      example: 1,
      description: "問題のID",
    }),
    question: z.string().openapi({
      example: "2 + 2は？",
      description: "問題文",
    }),
    correctAnswer: z.string().openapi({
      example: "4",
      description: "正解",
    }),
    difficulty: z.number().min(1).max(3).openapi({
      example: 1,
      description: "難易度（1-3）",
    }),
    category: z.string().openapi({
      example: "数学",
      description: "カテゴリ",
    }),
  })
  .openapi("Problem");

// 新規作成 (POST) 用のスキーマ
export const createProblemSchema = z
  .object({
    question: z
      .string()
      .min(1, "問題は1文字以上で入力してください")
      .max(100, "問題は100文字以内で入力してください")
      .openapi({
        example: "2 + 2は？",
        description: "問題文",
      }),
    correctAnswer: z
      .string()
      .min(1, "正解は1文字以上で入力してください")
      .max(100, "正解は100文字以内で入力してください")
      .openapi({
        example: "4",
        description: "正解",
      }),
    difficulty: z
      .number()
      .min(1)
      .max(3)
      .openapi({
        example: 1,
        description: "難易度（1-3）",
      }),
    category: z
      .string()
      .min(1, "カテゴリは1文字以上で入力してください")
      .max(100, "カテゴリは100文字以内で入力してください")
      .openapi({
        example: "数学",
        description: "カテゴリ",
      }),
  })
  .openapi("CreateProblem");

// ID 指定 (GET, PUT, DELETE のパラメータ) 用のスキーマ
export const problemIdSchema = z
  .object({
    id: z
      .string()
      .openapi({
        param: {
          name: "id",
          in: "path",
        },
        example: "1",
        description: "問題のID",
      }),
  })
  .openapi("ProblemId");
