using System;
using Elements.Core;
using Elements.Assets;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System.Collections.Generic;
using System.Linq;
using Renderite.Shared;

#if DEBUG
	using ResoniteHotReloadLib;
#endif

namespace CustomTextureSettings;

public class CustomTextureSettingsMod : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.1.0";
	internal const string NAME_CONSTANT = "CustomTextureSettings";
	internal const string AUTHOR_CONSTANT = "ultrawidegamer";
	internal const string LINK_CONSTANT = "https://github.com/ultrawidegamer/CustomTextureSettings";
	public override string Version => VERSION_CONSTANT;
	public override string Name => NAME_CONSTANT;
	public override string Author => AUTHOR_CONSTANT;
	public override string Link => LINK_CONSTANT;

	const string HarmonyId = "com.ultrawidegamer.CustomTextureSettings";

	internal static readonly List<ImportItem?> ItemPaths = new List<ImportItem?>();

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> ModEnabled = new ModConfigurationKey<bool>("Enabled", "Should the mod be enabled?", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> DirectLoadEnabled = new ModConfigurationKey<bool>("Direct Load", "Should your texture have direct load enabled?", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> CrunchCompressedEnabled = new ModConfigurationKey<bool>("Crunch Compressed", "Should your texture be crunch compressed?", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> MipMapsEnabled = new ModConfigurationKey<bool>("Mip Maps", "Should your texture have mip maps enabled?", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> UncompressedEnabled = new ModConfigurationKey<bool>("Uncompressed", "Should your texture be uncompressed?", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> ForceExactVariantEnabled = new ModConfigurationKey<bool>("Force Exact Variant", "Should your texture have force exact variant enabled?", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> IsNormalMap = new ModConfigurationKey<bool>("Is Normal Map", "Should your texture be a normal map?", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> KeepOriginalMipMaps = new ModConfigurationKey<bool>("Keep Original Mip Maps", "Should your texture keep original mip maps?", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> Readable = new ModConfigurationKey<bool>("Readable", "Should your texture be readable?", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<float> MipMapBias = new ModConfigurationKey<float>("Mip Map Bias", "Mip map bias for your texture", () => 0);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<float> PowerOfTwoAlignThreshold = new ModConfigurationKey<float>("Power Of Two Align Threshold", "Power of two align threshold for your texture", () => 0.05f);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<TextureFilterMode?> FilterMode = new ModConfigurationKey<TextureFilterMode?>("Texture Filter Mode", "Texture filter mode for your texture", () => TextureFilterMode.Bilinear);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<int?> AnisotropicLevel = new ModConfigurationKey<int?>("Anisotropic Level", "Anisotropic level for your texture", () => null);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<int?> MinSize = new ModConfigurationKey<int? >("Min Size", "Min size for your texture", () => null);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<int?> MaxSize = new ModConfigurationKey<int?>("Max Size", "Max size for your texture", () => null);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<TextureCompression?> PreferredFormat = new ModConfigurationKey<TextureCompression?>("Preferred Format", "Preferred format for your texture", () => TextureCompression.BC3_Crunched);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<ColorProfile?> PreferredProfile = new ModConfigurationKey<ColorProfile?>("Preferred Profile", "Preferred profile for your texture", () => ColorProfile.sRGB);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<TextureWrapMode> WrapModeU = new ModConfigurationKey<TextureWrapMode>("Wrap Mode U", "Set the Wrap Mode U for your texture", () => TextureWrapMode.Repeat);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<TextureWrapMode> WrapModeV = new ModConfigurationKey<TextureWrapMode>("Wrap Mode V", "Set the Wrap Mode V for your texture", () => TextureWrapMode.Repeat);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<Filtering> MipMapFilter = new ModConfigurationKey<Filtering>("Mip Map Filter", "Mip Map Filter for your texture", () => Filtering.Box);


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

			var slotName = targetSlot.Name_Field ?? "";
			var match = ItemPaths.FirstOrDefault(path => path?.itemName == slotName);

			if (match is null) return true;

			var staticTexture = targetSlot.GetComponent<StaticTexture2D>();

			staticTexture.DirectLoad.Value = Config.GetValue(DirectLoadEnabled);
			staticTexture.CrunchCompressed.Value = Config.GetValue(CrunchCompressedEnabled);
			staticTexture.MipMaps.Value = Config.GetValue(MipMapsEnabled);
			staticTexture.Uncompressed.Value = Config.GetValue(UncompressedEnabled);
			staticTexture.ForceExactVariant.Value = Config.GetValue(ForceExactVariantEnabled);
			staticTexture.IsNormalMap.Value = Config.GetValue(IsNormalMap);
			staticTexture.KeepOriginalMipMaps.Value = Config.GetValue(KeepOriginalMipMaps);
			staticTexture.Readable.Value = Config.GetValue(Readable);
			staticTexture.FilterMode.Value = Config.GetValue(FilterMode);
			staticTexture.MipMapBias.Value = Config.GetValue(MipMapBias);
			staticTexture.PowerOfTwoAlignThreshold.Value = Config.GetValue(PowerOfTwoAlignThreshold);
			staticTexture.AnisotropicLevel.Value = Config.GetValue(AnisotropicLevel);
			staticTexture.MinSize.Value = Config.GetValue(MinSize);
			staticTexture.MaxSize.Value = Config.GetValue(MaxSize);
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

			ui.Canvas.UnitScale.Value = 1.05f;

			LocaleString text = "Image / Texture <size=50%>(Custom)";
			Button directLoadButton = ui.Button(in text);

			directLoadButton.LocalPressed += (IButton button, ButtonEventData eventData) => {
				try {
					foreach (var path in __instance.Paths) {
						ItemPaths.Add(path);
					}
					Traverse.Create(__instance)
						.Method("Preset_Image", new[] { typeof(IButton), typeof(ButtonEventData) })
						.GetValue(button, eventData);
				} catch (Exception error) {
					directLoadButton.LabelText = $"<alpha=red>Failed to import. Check logs.";
					Error(error);
				}
			};
		}
	}
}
