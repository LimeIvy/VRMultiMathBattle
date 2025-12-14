import { OpenAPIHono } from "@hono/zod-openapi";
import { cors } from "hono/cors";
import { swaggerUI } from "@hono/swagger-ui";
import problemsRoutes from "./routes/problems";

type Bindings = {
  DB: D1Database;
};

const app = new OpenAPIHono<{ Bindings: Bindings }>();

app.use(
  "*",
  cors({
    origin: "*",
  })
);

// Hello エンドポイント
app.get("/hello", (c) => {
  return c.json({ message: "Hello Hono!" });
});

// Problems エンドポイントをマウント
app.route("/", problemsRoutes);

// OpenAPI JSON エンドポイント
app.doc("/openapi.json", {
  openapi: "3.0.0",
  info: {
    version: "1.0.0",
    title: "VR Multi Math Battle API",
    description: "VR Multi Math Battle のバックエンドAPI",
  },
  servers: [
    {
      url: "http://localhost:8787",
      description: "ローカル開発環境",
    },
  ],
});

// Swagger UI エンドポイント
app.get("/docs", swaggerUI({ url: "/openapi.json" }));

export type AppType = typeof app;
export default app;
