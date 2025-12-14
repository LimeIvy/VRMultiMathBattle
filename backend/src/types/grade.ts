import { z } from "@hono/zod-openapi";

// 採点リクエストスキーマ
export const GradeRequestSchema = z
  .object({
    problemId: z.number().openapi({
      example: 1,
      description: "問題のID",
    }),
    imageBase64: z.string().openapi({
      example: "/9j/4AAQSkZJRgABAQEAYABgAAD...",
      description: "Base64エンコードされた画像データ（JPEG/PNG）",
    }),
    playerName: z.string().optional().openapi({
      example: "Player1",
      description: "プレイヤー名（オプション）",
    }),
  })
  .openapi("GradeRequest");

// 採点レスポンススキーマ
export const GradeResponseSchema = z
  .object({
    id: z.number().openapi({
      example: 1,
      description: "採点結果のID",
    }),
    isCorrect: z.boolean().openapi({
      example: true,
      description: "解答が正解かどうか",
    }),
    feedback: z.string().openapi({
      example: "正解です！計算が正確でした。",
      description: "AIからのフィードバック",
    }),
  })
  .openapi("GradeResponse");
