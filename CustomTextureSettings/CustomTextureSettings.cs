using System;
using Elements.Core;
using Elements.Assets;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System.Collections.Generic;
using System.Linq;

#if DEBUG
	using ResoniteHotReloadLib;
#endif

namespace CustomTextureSettings;

public class CustomTextureSettingsMod : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0";
	public override string Name => "CustomTextureSettings";
	public override string Author => "ultrawidegamer";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/ultrawidegamer/CustomTextureSettings";

	const string HarmonyId = "com.ultrawidegamer.CustomTextureSettings";

	internal static readonly List<ImportItem?> ItemPaths = new List<ImportItem?>();

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> ModEnabled = new ModConfigurationKey<bool>("Enabled", "Should the mod be enabled?", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> DirectLoadEnabled = new ModConfigurationKey<bool>("Direct Load", "Should your image have direct load enabled?", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> CrunchCompressedEnabled = new ModConfigurationKey<bool>("Crunch Compressed", "Should your image be crunch compressed?", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> MipMapsEnabled = new ModConfigurationKey<bool>("Mip Maps", "Should your image have mip maps enabled?", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> UncompressedEnabled = new ModConfigurationKey<bool>("Uncompressed", "Should your image be uncompressed?", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<TextureFilterMode> FilterMode = new ModConfigurationKey<TextureFilterMode>("Texture Filter Mode", "Texture filter mode for your image", () => TextureFilterMode.Bilinear);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<int?> AnisotropicLevel = new ModConfigurationKey<int?>("Anisotropic Level", "Anisotropic level for your image", () => 0);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<TextureCompression?> PreferredFormat = new ModConfigurationKey<TextureCompression?>("Preferred Format", "Preferred format for your image", () => TextureCompression.BC3_Crunched);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<ColorProfile?> PreferredProfile = new ModConfigurationKey<ColorProfile?>("Preferred Profile", "Preferred profile for your image", () => ColorProfile.sRGB);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<TextureWrapMode> WrapModeU = new ModConfigurationKey<TextureWrapMode>("Wrap Mode U", "Set the Wrap Mode U for your image", () => TextureWrapMode.Repeat);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<TextureWrapMode> WrapModeV = new ModConfigurationKey<TextureWrapMode>("Wrap Mode V", "Set the Wrap Mode V for your image", () => TextureWrapMode.Repeat);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<Filtering> MipMapFilter = new ModConfigurationKey<Filtering>("Mip Map Filter", "Mip Map Filter for your image", () => Filtering.Box);


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

	[HarmonyPatch(typeof(ImageImporter), "SetupTextureProxyComponents")]
	class ImageImporter_SetupTextureProxyComponents_Patch {
		[HarmonyPrefix]
		private static bool SetupTextureProxyComponents(Slot targetSlot, IAssetProvider<Texture2D> texture, StereoLayout stereoLayout, ImageProjection projection, bool setupPhotoMetadata) {
			if (Config is null) return true;
			if (!Config.GetValue(ModEnabled)) return true;

			var slotName = targetSlot.NameField ?? "";
			var match = ItemPaths.FirstOrDefault(path => path?.itemName == slotName);

			if (match is null) return true;

			var staticTexture = targetSlot.GetComponent<StaticTexture2D>();

			staticTexture.DirectLoad.Value = Config.GetValue(DirectLoadEnabled);
			staticTexture.CrunchCompressed.Value = Config.GetValue(CrunchCompressedEnabled);
			staticTexture.MipMaps.Value = Config.GetValue(MipMapsEnabled);
			staticTexture.Uncompressed.Value = Config.GetValue(UncompressedEnabled);
			staticTexture.FilterMode.Value = Config.GetValue(FilterMode);
			staticTexture.AnisotropicLevel.Value = Config.GetValue(AnisotropicLevel);
			staticTexture.PreferredFormat.Value = Config.GetValue(PreferredFormat);
			staticTexture.PreferredProfile.Value = Config.GetValue(PreferredProfile);
			staticTexture.WrapModeU.Value = Config.GetValue(WrapModeU);
			staticTexture.WrapModeV.Value = Config.GetValue(WrapModeV);
			staticTexture.MipMapFilter.Value = Config.GetValue(MipMapFilter);

			ItemPaths.Remove(match);

			return true;
		}
	}

	[HarmonyPatch(typeof(ImageImportDialog), "OpenRoot")]
	class ImageImportDialog_OpenRoot_Patch {
		[HarmonyPostfix]
		private static void OpenRoot(ImageImportDialog __instance, UIBuilder ui) {
			if (Config is null) return;
			if (!Config.GetValue(ModEnabled)) return;

			LocaleString text = "DirectLoad";
			Button directLoadButton = ui.Button(in text);

			directLoadButton.LocalPressed += (IButton button, ButtonEventData eventData) => {
				try {
					foreach (var path in __instance.Paths) {
						ItemPaths.Add(path);
						Traverse.Create(__instance)
							.Method("Preset_Image", new[] { typeof(IButton), typeof(ButtonEventData) })
							.GetValue(button, eventData);
					}
				} catch (Exception error) {
					directLoadButton.LabelText = $"<alpha=red>Failed to import. Check logs.";
					Error(error);
				}
			};
		}
	}
}
