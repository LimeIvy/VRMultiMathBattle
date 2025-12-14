import type { Config } from "drizzle-kit";

export default {
  out: "./src/migrations",
  schema: "./src/database/schema.ts",
  dialect: "sqlite",
  driver: "d1-http",
} satisfies Config;
