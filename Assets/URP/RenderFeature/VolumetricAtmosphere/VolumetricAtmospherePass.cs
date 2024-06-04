using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class VolumetricAtmospherePass : ScriptableRenderPass
{
    private ProfilingSampler _profilingSampler;
    private RTHandle _rtColor, _rtTempColor ,_rtTempColor2;
    private Material _blitMat;
    private RenderTexture _opticDepthTex_external;
    private ComputeShader _baker;
    private VolumetricAtmosphereBlendingComponent _volumeSettings;
    private bool _realtime;
    public VolumetricAtmospherePass(string name)
    {
        _profilingSampler = new ProfilingSampler(name);
    }
    public void SetData(RTHandle colorHandle,ComputeShader baker, RenderTexture opticalDepthTex, Material blitMat, bool realtime)
    {
        _rtColor = colorHandle;
        _baker = baker;
        _opticDepthTex_external = opticalDepthTex;
        _blitMat = blitMat;
        _volumeSettings = VolumeManager.instance.stack.GetComponent<VolumetricAtmosphereBlendingComponent>();
        _realtime = realtime;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTextureDescriptor camTargetDesc = renderingData.cameraData.cameraTargetDescriptor;
        camTargetDesc.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref _rtTempColor, camTargetDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempColor");
        RenderingUtils.ReAllocateIfNeeded(ref _rtTempColor2, camTargetDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempColor2");
        ConfigureTarget(_rtColor);
        CheckValidation();
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        if (_volumeSettings == null)
            return;
        if (!_volumeSettings.IsActive())
            return;
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            _blitMat.SetVector("_SphereMaskCenter", _volumeSettings.BlendCenter.value);
            _blitMat.SetFloat("_SphereMaskRadius", _volumeSettings.BlendRaidus.value);
            _blitMat.SetFloat("_SphereMaskBlend", _volumeSettings.BlendFalloff.value);

            LocalKeyword enableRealtime = new LocalKeyword(_blitMat.shader, "_USE_REALTIME");
            if (_realtime)
                _blitMat.EnableKeyword(enableRealtime);
            else
                _blitMat.DisableKeyword(enableRealtime);

            _blitMat.SetTexture("_CameraOpaqueTexture", renderingData.cameraData.renderer.cameraColorTargetHandle);
            _blitMat.SetTexture("_CameraDepthTexture", renderingData.cameraData.renderer.cameraDepthTargetHandle);
            _blitMat.SetTexture("_OpticalDepthTexture", _opticDepthTex_external);
            _blitMat.SetFloat("_Camera_Near", renderingData.cameraData.camera.nearClipPlane);
            _blitMat.SetFloat("_Camera_Far", renderingData.cameraData.camera.farClipPlane);
            _blitMat.SetFloat("_EarthRadius", _volumeSettings.EarthRadius.value);
            _blitMat.SetInt("_NumOpticalDepthSample", _volumeSettings.OpticalDepthSamples.value);
            _blitMat.SetInt("_NumInScatteringSample", _volumeSettings.InscatteringSamples.value);

            _blitMat.SetFloat("_SphereMaskDistortBlend", _volumeSettings.BlendDistortFalloff.value);
            _blitMat.SetFloat("_DistorsionStrength", _volumeSettings.BlendDistortStrength.value);

            LocalKeyword enableRayleigh = new LocalKeyword(_blitMat.shader, "_USE_RAYLEIGH");
            if (_volumeSettings.EnableRayleighScattering.value)
                _blitMat.EnableKeyword(enableRayleigh);
            else
                _blitMat.DisableKeyword(enableRayleigh);
            _blitMat.SetFloat("_Rs_Thickness", _volumeSettings.AtmosphereHeight.value);
            _blitMat.SetFloat("_Rs_DensityFalloff", _volumeSettings.AtmosphereDensityFalloff.value);

            _blitMat.SetFloat("_Rs_Absorbsion_1", _volumeSettings.AtmosphereUniformAbsorbsion_1.value);
            _blitMat.SetFloat("_Rs_DensityMultiplier_1", _volumeSettings.AtmosphereDensityMultiplier_1.value);
            _blitMat.SetFloat("_Rs_ChannelSplit_1", _volumeSettings.AtmosphereChannelSplit_1.value);
            _blitMat.SetColor("_Rs_ScatterWeight_1", _volumeSettings.AtmosphereAbsorbsionWeightPerChannel_1.value);
            _blitMat.SetColor("_Rs_InsColor_1", _volumeSettings.AtmosphereInscatteringTint_1.value);

            _blitMat.SetFloat("_Rs_Absorbsion_2", _volumeSettings.AtmosphereUniformAbsorbsion_2.value);
            _blitMat.SetFloat("_Rs_DensityMultiplier_2", _volumeSettings.AtmosphereDensityMultiplier_2.value);
            _blitMat.SetFloat("_Rs_ChannelSplit_2", _volumeSettings.AtmosphereChannelSplit_2.value);
            _blitMat.SetColor("_Rs_ScatterWeight_2", _volumeSettings.AtmosphereAbsorbsionWeightPerChannel_2.value);
            _blitMat.SetColor("_Rs_InsColor_2", _volumeSettings.AtmosphereInscatteringTint_2.value);


            LocalKeyword enableMie = new LocalKeyword(_blitMat.shader, "_USE_MIE");
            if (_volumeSettings.EnableMieScattering.value)
                _blitMat.EnableKeyword(enableMie);
            else
                _blitMat.DisableKeyword(enableMie);
            _blitMat.SetFloat("_Ms_Thickness", _volumeSettings.AerosolsHeight.value);
            _blitMat.SetFloat("_Ms_DensityFalloff", _volumeSettings.AerosolsDensityFalloff.value);

            _blitMat.SetFloat("_Ms_Absorbsion_1", _volumeSettings.AerosolsUniformAbsorbsion_1.value);
            _blitMat.SetFloat("_Ms_DensityMultiplier_1", _volumeSettings.AerosolsDensityMultiplier_1.value);
            _blitMat.SetFloat("_Ms_Anisotropic_1", _volumeSettings.AerosolsAnistropic_1.value);
            _blitMat.SetColor("_Ms_InsColor_1", _volumeSettings.AerosolsInscatteringTint_1.value);


            _blitMat.SetFloat("_Ms_Absorbsion_2", _volumeSettings.AerosolsUniformAbsorbsion_2.value);
            _blitMat.SetFloat("_Ms_DensityMultiplier_2", _volumeSettings.AerosolsDensityMultiplier_2.value);
            _blitMat.SetFloat("_Ms_Anisotropic_2", _volumeSettings.AerosolsAnistropic_2.value);
            _blitMat.SetColor("_Ms_InsColor_2", _volumeSettings.AerosolsInscatteringTint_2.value);

            _blitMat.SetInt("_VolumeOnly", _volumeSettings.VolumePassOnly.value ? 1 : 0);
            Blitter.BlitCameraTexture(cmd, _rtColor, _rtTempColor, _blitMat, 0);
            Blitter.BlitCameraTexture(cmd, _rtTempColor, _rtColor);


            LocalKeyword enableMask = new LocalKeyword(_blitMat.shader, "_USE_MASK");
            if (_volumeSettings.EnableMask.value)
                _blitMat.EnableKeyword(enableMask);
            else
                _blitMat.DisableKeyword(enableMask);

            LocalKeyword enableDistorsion = new LocalKeyword(_blitMat.shader, "_USE_DISTORSION");
            if (_volumeSettings.EnableDistorsion.value && _volumeSettings.EnableMask.value)
                _blitMat.EnableKeyword(enableDistorsion);
            else
                _blitMat.DisableKeyword(enableDistorsion);
          

            if (_volumeSettings.EnableDistorsion.value) 
            {
                Blitter.BlitCameraTexture(cmd, _rtColor, _rtTempColor2, _blitMat, 1);
                Blitter.BlitCameraTexture(cmd, _rtTempColor2, _rtColor);
            }
  
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
    private float
        numOpticalDepthSample_temp,
        rs_thickness_temp,
        rs_densityFalloff_temp,
        ms_thickness_temp,
        ms_densityFalloff_temp,
        earthRadius_temp;
    private void CheckValidation() {

        if (
            numOpticalDepthSample_temp != _volumeSettings.OpticalDepthSamples.value
            || rs_thickness_temp != _volumeSettings.AtmosphereHeight.value
            || rs_densityFalloff_temp != _volumeSettings.AtmosphereDensityFalloff.value
            || ms_thickness_temp != _volumeSettings.AerosolsHeight.value
            || ms_densityFalloff_temp != _volumeSettings.AerosolsDensityFalloff.value
            || earthRadius_temp != _volumeSettings.EarthRadius.value
            )
            UpdateBakedTexture();
        numOpticalDepthSample_temp = _volumeSettings.OpticalDepthSamples.value;
        rs_thickness_temp = _volumeSettings.AtmosphereHeight.value;
        rs_densityFalloff_temp = _volumeSettings.AtmosphereDensityFalloff.value;
        ms_thickness_temp = _volumeSettings.AerosolsHeight.value;
        ms_densityFalloff_temp = _volumeSettings.AerosolsDensityFalloff.value;
        earthRadius_temp = _volumeSettings.EarthRadius.value;
    }
    public void UpdateBakedTexture() 
    {
        if (_baker == null)
            return;
        if (_opticDepthTex_external == null)
            return;
        int res = _opticDepthTex_external.width;
        if (res <= 8)
            return;
        _baker.SetTexture(0, "_LookupRT", _opticDepthTex_external);
        _baker.SetInt("_Resolusion", res);
        _baker.SetInt("_NumOpticalDepthSample", _volumeSettings.OpticalDepthSamples.value);
        _baker.SetFloat("_RS_Thickness", _volumeSettings.AtmosphereHeight.value);
        _baker.SetFloat("_RS_DensityFalloff", _volumeSettings.AtmosphereDensityFalloff.value);
        _baker.SetFloat("_MS_Thickness", _volumeSettings.AerosolsHeight.value);
        _baker.SetFloat("_MS_DensityFalloff", _volumeSettings.AerosolsDensityFalloff.value);
        _baker.SetFloat("_EarthRadius", _volumeSettings.EarthRadius.value);
        _baker.Dispatch(0, Mathf.CeilToInt(res / 8), Mathf.CeilToInt(res / 8), 1);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }
    public void Dispose()
    {
        _rtColor?.Release();
        _rtTempColor?.Release();
        _rtTempColor2?.Release();
    }
}
