const baseUrl = process.env.BASE_URL ?? "http://localhost:8787";
const runGradeTest = process.env.RUN_GRADE_TEST === "true";
const gradeImageBase64 = process.env.GRADE_IMAGE_BASE64 ?? "";
const playerName = process.env.PLAYER_NAME ?? "IntegrationTester";

const logStep = (message) => {
  console.log(`\n[Integration Test] ${message}`);
};

const expectOk = async (response, label) => {
  if (!response.ok) {
    const body = await response.text();
    throw new Error(`${label} failed: ${response.status} ${response.statusText}\n${body}`);
  }
};

const postJson = async (url, body) => {
  const response = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  await expectOk(response, `POST ${url}`);
  return response.json();
};

const getJson = async (url) => {
  const response = await fetch(url);
  await expectOk(response, `GET ${url}`);
  return response.json();
};

const run = async () => {
  logStep(`Base URL: ${baseUrl}`);

  logStep("Create a problem");
  const created = await postJson(`${baseUrl}/problems`, {
    question: "3 + 9 は？",
    correctAnswer: "12",
    difficulty: 1,
    category: "算数",
  });
  console.log("Created:", created);

  logStep("Get all problems");
  const problems = await getJson(`${baseUrl}/problems`);
  console.log(`Total problems: ${problems.length}`);

  logStep("Get random problem");
  const randomProblem = await getJson(`${baseUrl}/problems/random`);
  console.log("Random:", randomProblem);

  if (runGradeTest) {
    if (!gradeImageBase64) {
      throw new Error("RUN_GRADE_TEST=true requires GRADE_IMAGE_BASE64");
    }

    logStep("Grade a captured image");
    const grade = await postJson(`${baseUrl}/grade`, {
      problemId: created.id,
      imageBase64: gradeImageBase64,
      playerName,
    });
    console.log("Grade result:", grade);
  } else {
    logStep("Grade test skipped (set RUN_GRADE_TEST=true to enable)");
  }

  logStep("Integration test completed");
};

run().catch((error) => {
  console.error("\n[Integration Test] Failed");
  console.error(error);
  process.exitCode = 1;
});
