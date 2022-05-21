using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine.UI;

namespace UnityCtrlBackspace
{
    [BepInPlugin("com.randomguyjci.unityctrlbackspace", "Ctrl-Backspace For Unity", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(PatchInputField));
        }

        public static class PatchInputField
        {
            private static bool isCtrlHeld = false;
            
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(InputField), "KeyPressed")]
            public static IEnumerable<CodeInstruction> KeyPressedTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(InputField), "Backspace")))
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldloc_S, 4),
                        new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(typeof(PatchInputField), nameof(isCtrlHeld))))
                    .InstructionEnumeration();
            }
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(InputField), "Backspace")]
            public static bool Prefix(InputField __instance)
            {
                if (!__instance.m_ReadOnly)
                {
                    if (__instance.hasSelection)
                    {
                        __instance.Delete();
                        __instance.UpdateTouchKeyboardFromEditChanges();
                        __instance.SendOnValueChangedAndUpdateLabel();
                    }
                    else if (__instance.caretPositionInternal > 0 && __instance.caretPositionInternal - 1 < __instance.text.Length)
                    {
                        if (isCtrlHeld)
                        {
                            BackspaceWord(ref __instance);
                        }
                        else
                        {
                            __instance.m_Text = __instance.text.Remove(__instance.caretPositionInternal - 1, 1);
                            __instance.caretSelectPositionInternal = --__instance.caretPositionInternal;
                        }
                        __instance.UpdateTouchKeyboardFromEditChanges();
                        __instance.SendOnValueChangedAndUpdateLabel();
                    }
                }
                return false;
            }

            private static void BackspaceWord(ref InputField __instance)
            {
                var newCaretPosition = ModifiedFindPrevWordBegin(__instance.m_Text, __instance.caretPositionInternal);
                var moveLength = __instance.caretPositionInternal - newCaretPosition;

                __instance.m_Text = __instance.text.Remove(newCaretPosition, moveLength);
                __instance.caretPositionInternal = __instance.caretSelectPositionInternal = newCaretPosition;
            }

            private static int ModifiedFindPrevWordBegin(string text, int caretPosition)
            {
                var lastCaretPosition = caretPosition - 1;

                if (char.IsWhiteSpace(text[lastCaretPosition]))
                {
                    for (; lastCaretPosition > 0; lastCaretPosition--)
                        if (!char.IsWhiteSpace(text[lastCaretPosition])) break;
                }
                
                if (char.IsLetterOrDigit(text[lastCaretPosition]))
                {
                    for (; lastCaretPosition > 0; lastCaretPosition--)
                        if (!char.IsLetterOrDigit(text[lastCaretPosition - 1])) break;
                }
                else
                {
                    for (; lastCaretPosition > 0; lastCaretPosition--)
                        if (char.IsLetterOrDigit(text[lastCaretPosition - 1]) || char.IsWhiteSpace(text[lastCaretPosition - 1])) break;
                }
                
                return lastCaretPosition;
            }
        }
    }
}
