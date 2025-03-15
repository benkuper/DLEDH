using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyController : MonoBehaviour
{

    [SerializeField] VolumeProfile volumeProfile;
    VolumetricClouds vClouds;

    [Range(0,1)]
    public float densityMultiplier;
    [Range(0, 1)]
    public float shapeFactor;
    [Range(0, 10)]
    public float shapeScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (volumeProfile.TryGet<VolumetricClouds>(out vClouds))
        {
            //vClouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
            vClouds.densityMultiplier.value = densityMultiplier;
            vClouds.shapeFactor.value = shapeFactor;
            vClouds.shapeScale.value = shapeScale;
        }
    }
}
