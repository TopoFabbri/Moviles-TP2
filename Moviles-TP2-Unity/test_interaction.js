import { GoogleGenAI } from "@google/genai";

const ai = new GoogleGenAI();

try {
  const interaction = await ai.interactions.create({
    model: "gemini-4-hydrogen",
    input: "Explain how AI works in a few words",
  });
  console.log("Interaction reply:\n" + interaction.output_text);
} catch (error) {
  console.error("Error creating interaction:", error);
}
