namespace Jadderline {
    public class Jadderline {
        const float DeltaTime = 0.0166667f;
        const float FrictionNorm = 650f;
        const float FrictionOverMax = 260f;
        const float FrictionNormHold = FrictionNorm / 2;
        const float FrictionOverMaxHold = FrictionOverMax / 2;

        // Most inputs are self explanatory, though for direction, false is left and true is right
        // Additionally, jelly2 is the one on cooldown (jelly1 is the one about to be grabbed, and as such, doesnt need to be inputted)
        // The additional inputs may or may not need commas, not 100% sure
        // This doesnt go back a frame if an impossible frame is reached, but ive never seen it ever go back in jadderline so its not too much of a priority currently
        public static string Run(float playerPos, float playerSpeed, float jelly2Pos, int ladders, bool direction, bool moveOnly, string additionalInputs) {
            jelly2Pos = float.Round(jelly2Pos) + 9.5f; // Jelly positions should be on the right edge
            playerPos -= 4f; // And the player position should be on the left edge
            float jelly1Pos = playerPos; // Since this is the one we are about to grab
            List<bool[]> inputs = new List<bool[]>();
            for (int i = 0; i < ladders; i++) {
                // Get all 512 candidates
                List<bool[]> potential = new List<bool[]>();
                for (int j = 0; j < 512; j++) {
                    potential.Add(ToBits(j));
                }
                List<float> results = new List<float>();
                foreach (var inp in potential) {
                    results.Add(Eval(inp, playerPos, playerSpeed, jelly1Pos, jelly2Pos, direction));
                }
                int max = results.IndexOf(results.Max());
                if (results[max] == float.NegativeInfinity) {
                    throw new ArgumentException("Malformed input or impossible jelly ladder"); // Is this actually the right exception to use? No clue
                }
                inputs.Add(potential[max]);
                (playerPos, playerSpeed, jelly1Pos) = MoveVars(potential[max], playerPos, playerSpeed, jelly1Pos, direction); // save the result of the chosen input
                jelly2Pos = float.Round(jelly1Pos) + 13.5f; // Make jelly1 the new jelly2
                jelly1Pos = playerPos;
            }
            return Format(inputs, moveOnly, direction, additionalInputs);
        }

        // Gets the distance jelly1 has moved while ensuring the player can still grab jelly2
        private static float Eval(bool[] inputs, float playerPos, float playerSpeed, float jelly1Pos, float jelly2Pos, bool direction) {
            (float playerPosNew, float playerSpeedNew, float jelly1PosNew) = MoveVars(inputs, playerPos, playerSpeed, jelly1Pos, direction);
            if (float.Abs(playerPosNew) >= float.Abs(jelly2Pos)) { // Went past jelly   
                return float.NegativeInfinity;
            } else {
                return float.Abs(jelly1PosNew - jelly1Pos); // We want to maximize this and not player movement in order to prioritize 13px jadders when possible
                                                            // (though the rust ver maximizes player movement so idk)
            }
        }

        // Actually calculates the inputs
        private static (float, float, float) MoveVars(bool[] inputs, float playerPos, float playerSpeed, float jelly1Pos, bool direction) {
            // Frame of movement on last frame of StPickup
            playerPos += playerSpeed * DeltaTime;
            // 8 frames of holding the jelly
            for (int i = 0; i < 8; i++) {
                (playerPos, playerSpeed) = MoveStep(inputs[i], playerPos, playerSpeed, true, direction);
            }
            // Release jelly1, which will be at the player's current positition when dropped
            jelly1Pos = playerPos;
            // Frame of movement when you release the jelly
            (playerPos, playerSpeed) = MoveStep(inputs[8], playerPos, playerSpeed, false, direction);
            return (playerPos, playerSpeed, jelly1Pos);
        }

        // Calculates one frame of movement
        private static (float, float) MoveStep(bool input, float playerPos, float playerSpeed, bool holding, bool direction) {
            float frictionNorm;
            float frictionOverMax;
            float max;
            if (holding) {
                frictionNorm = FrictionNormHold;
                frictionOverMax = FrictionOverMaxHold;
                max = 108f;
            } else {
                frictionNorm = FrictionNorm;
                frictionOverMax = FrictionOverMax;
                max = 90f;
            }
            playerSpeed = float.Abs(playerSpeed);
            if (!input) { // Holding neutral
                playerSpeed -= frictionNorm * DeltaTime;
                if (playerSpeed < 0f) {
                    playerSpeed = 0f;
                }
            } else if (playerSpeed <= max) { // Coming up to max speed
                playerSpeed += frictionNorm * DeltaTime;
                if (playerSpeed > max) {
                    playerSpeed = max;
                }
            } else { // Over max speed
                playerSpeed -= frictionOverMax * DeltaTime;
                if (playerSpeed < max) {
                    playerSpeed = max;
                }
            }
            if (!direction) { // Flip speed back if moving left
                playerSpeed *= -1;
            }
            playerPos += playerSpeed * DeltaTime;
            return (playerPos, playerSpeed);
        }

        // Converts an int into a bool[9]
        private static bool[] ToBits(int num) {
            bool[] bits = new bool[9];
            for (int i = 0; i < 9; i++) {
                bits[i] = (num & 1) != 0;
                num >>= 1;
            }
            return bits;
        }

        // Formats the inputs to be copy and pasted into Studio
        private static string Format(List<bool[]> inputs, bool moveOnly, bool direction, string additionalInputs) {
            string result = "";
            string dirString;
            if (direction) {
                dirString = "R";
            } else {
                dirString = "L";
            }
            if (moveOnly) {
                dirString = "M" + dirString;
            }
            foreach (var input in inputs) {
                List<(int, bool)> formatted = new List<(int, bool)>();
                formatted.Add((13, false));
                for (int i = 0; i < 8; i++) {
                    int last = formatted.Count - 1;
                    if (formatted[last].Item2 == input[i]) {
                        formatted[last] = (formatted[last].Item1 + 1, input[i]);
                    } else {
                        formatted.Add((1, input[i]));
                    }
                }
                foreach (var f in formatted) {
                    if (f.Item2) {
                        result += $"{f.Item1}G{additionalInputs}{dirString}\n";
                    } else {
                        result += $"{f.Item1}G{additionalInputs}\n";
                    }
                }
                string downString;
                if (moveOnly) {
                    downString = "MD";
                } else {
                    downString = "D";
                }
                if (input[8]) {
                    result += $"1{additionalInputs}{dirString}{downString}\n";
                } else {
                    result += $"1{additionalInputs}{downString}\n";
                }
            }
            return result;
        }
    }
}