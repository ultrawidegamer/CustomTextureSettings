using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

#if DEBUG
	using ResoniteHotReloadLib;
#endif

namespace DirectLoadOnImport;

public class DirectLoadOnImportMod : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "DirectLoadOnImport";
	public override string Author => "ultrawidegamer";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/ultrawidegamer/DirectLoadOnImport";

	const string HarmonyId = "com.ultrawidegamer.DirectLoadOnImport";

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> enabled = new ModConfigurationKey<bool>("enabled", "Should the mod be enabled", () => true);

	private static ModConfiguration? Config;

	public override void OnEngineInit() {
		Init(this);		

		#if DEBUG
			HotReloader.RegisterForHotReload(this);
		#endif
	}

	private static void Init(ResoniteMod instance) {
		Config = instance?.GetConfiguration();

		if (Config is null) return;

		Config.Save(true);
		Harmony harmony = new Harmony(HarmonyId);
		harmony.PatchAll();
	}

	#if DEBUG
		public static void BeforeHotReload() {
			Harmony harmony = new Harmony(HarmonyId);
			harmony.UnpatchAll(HarmonyId);
		}

		public static void OnHotReload(ResoniteMod instance) {
			Init(instance);
		}
	#endif



	[HarmonyPatch(typeof(ImageImportDialog), "OpenRoot")]
	class ImageImportDialog_OpenRoot_Patch {
		[HarmonyPostfix]
		private static void OpenRoot(ImageImportDialog __instance, UIBuilder ui) {
			if (Config is null) return;
			if (!Config.GetValue(enabled)) return;

			Msg("Postfix of ImageImportDialog");

			LocaleString text = "DirectLoad";
			Button directLoadButton = ui.Button(in text);

			directLoadButton.LocalPressed += (IButton button, ButtonEventData eventData) => {
				try {
					Msg("Trying to import image");
				} catch (Exception error) {
					directLoadButton.LabelText = $"<alpha=red>Failed to import. Check logs.";
					Error(error);
				}
			};
		}
	}
}
