CREATE TABLE `problems` (
	`id` integer PRIMARY KEY AUTOINCREMENT NOT NULL,
	`question` text NOT NULL,
	`correct_answer` text NOT NULL,
	`difficulty` integer DEFAULT 1,
	`category` text DEFAULT 'General'
);
