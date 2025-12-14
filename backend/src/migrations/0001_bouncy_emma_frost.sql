CREATE TABLE `grades` (
	`id` integer PRIMARY KEY AUTOINCREMENT NOT NULL,
	`problem_id` integer NOT NULL,
	`player_name` text,
	`is_correct` integer NOT NULL,
	`feedback` text NOT NULL,
	FOREIGN KEY (`problem_id`) REFERENCES `problems`(`id`) ON UPDATE no action ON DELETE no action
);
--> statement-breakpoint
PRAGMA foreign_keys=OFF;--> statement-breakpoint
CREATE TABLE `__new_problems` (
	`id` integer PRIMARY KEY AUTOINCREMENT NOT NULL,
	`question` text NOT NULL,
	`correct_answer` text NOT NULL,
	`difficulty` integer DEFAULT 1 NOT NULL,
	`category` text DEFAULT 'General' NOT NULL
);
--> statement-breakpoint
INSERT INTO `__new_problems`("id", "question", "correct_answer", "difficulty", "category") SELECT "id", "question", "correct_answer", "difficulty", "category" FROM `problems`;--> statement-breakpoint
DROP TABLE `problems`;--> statement-breakpoint
ALTER TABLE `__new_problems` RENAME TO `problems`;--> statement-breakpoint
PRAGMA foreign_keys=ON;