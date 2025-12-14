import { OpenAPIHono } from "@hono/zod-openapi";
import { drizzle } from "drizzle-orm/d1";
import { createRoute } from "@hono/zod-openapi";
import { z } from "@hono/zod-openapi";
import {
  createProblemSchema,
  ProblemSchema,
} from "../types/problems";
import { problemsTable } from "../database/schema";

type Bindings = {
  DB: D1Database;
};

const problemsApp = new OpenAPIHono<{ Bindings: Bindings }>();

// GET /problems エンドポイント: 全問題を取得
const getProblemsRoute = createRoute({
  method: "get",
  path: "/problems",
  tags: ["Problems"],
  summary: "全問題を取得",
  description: "データベースから全ての問題を取得します",
  responses: {
    200: {
      content: {
        "application/json": {
          schema: z.array(ProblemSchema),
        },
      },
      description: "問題のリスト",
    },
  },
});

problemsApp.openapi(getProblemsRoute, async (c) => {
  const db = drizzle(c.env.DB);
  const problems = await db.select().from(problemsTable).all();
  return c.json(problems, 200);
});

// POST /problems エンドポイント: 新規問題を作成
const createProblemRoute = createRoute({
  method: "post",
  path: "/problems",
  tags: ["Problems"],
  summary: "新規問題を作成",
  description: "新しい問題をデータベースに追加します",
  request: {
    body: {
      content: {
        "application/json": {
          schema: createProblemSchema,
        },
      },
    },
  },
  responses: {
    201: {
      content: {
        "application/json": {
          schema: ProblemSchema,
        },
      },
      description: "作成された問題",
    },
  },
});

problemsApp.openapi(createProblemRoute, async (c) => {
  const db = drizzle(c.env.DB);
  const { question, correctAnswer, difficulty, category } = c.req.valid("json");
  const newProblem = await db
    .insert(problemsTable)
    .values({ question, correctAnswer, difficulty, category })
    .returning();
  return c.json(newProblem[0], 201);
});

export default problemsApp;
