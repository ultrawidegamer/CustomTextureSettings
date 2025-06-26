using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace DirectLoadOnImport;

public class DirectLoadOnImportMod : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "DirectLoadOnImport";
	public override string Author => "ultrawidegamer";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/ultrawidegamer/DirectLoadOnImport";

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> enabled = new ModConfigurationKey<bool>("enabled", "Should the mod be enabled", () => true);

	private static ModConfiguration Config;

	public override void OnEngineInit() {
		Config = GetConfiguration();
		Config.Save(true);
		Harmony harmony = new Harmony("com.ultrawidegamer.DirectLoadOnImport");
		harmony.PatchAll();

		Debug("a debug log");
		Msg("a regular log");
		Warn("a warn log");
		Error("an error log");
	}

	////Example of how a HarmonyPatch can be formatted, Note that the following isn't a real patch and will not compile.
	//[HarmonyPatch(typeof(ClassNameHere), "MethodNameHere")]
	//class ClassNameHere_MethodNameHere_Patch {
	//	static void Postfix(ClassNameHere __instance) {
	//		Msg("Postfix from ExampleMod");
	//	}
	//}
}
