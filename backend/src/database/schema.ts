import { integer, sqliteTable, text } from "drizzle-orm/sqlite-core";

export const problemsTable = sqliteTable("problems", {
  id: integer("id").primaryKey({ autoIncrement: true }),
  question: text("question").notNull(),
  correctAnswer: text("correct_answer").notNull(),
  difficulty: integer("difficulty").notNull().default(1),
  category: text("category").notNull().default("General"),
});
