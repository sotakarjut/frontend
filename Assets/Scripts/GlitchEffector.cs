using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GlitchEffector : MonoBehaviour {

    public static GlitchEffector current;

    public ShaderEffect_Unsync unsyncEffect;
    public ShaderEffect_Tint tintEffect;
    public BWEffect bwEffect;
    public ShaderEffect_BleedingColors bleedingColorEffect;
    public ShaderEffect_CorruptedVram vramEffect;
    public bool isGlitching = false;
    bool effectsEnabled = false;
    float glitchTime = 0f;
    float glitchPower = 1f;
    float glitchedFor = 0f;

    private void Awake()
    {
        current = this;
    }

    void enableEffect(int effectNumber, bool enabled)
    {
        if (effectNumber == 0) unsyncEffect.enabled = enabled;
        if (effectNumber == 1) bleedingColorEffect.enabled = enabled;
        if (effectNumber == 2) vramEffect.enabled = enabled;
        if (effectNumber == 3) tintEffect.enabled = enabled;
        if (effectNumber == 4) bwEffect.enabled = enabled;
    }

    // Update is called once per frame
    void Update () {
		if (isGlitching)
        {
            if (!effectsEnabled)
            {
                effectsEnabled = true;
            }
            glitchTime += Time.deltaTime;
            glitchedFor += Time.deltaTime;
            if (glitchTime > 100f) glitchTime = 0f;
            if (Random.Range(0f,100f) > 95f)
            {
                glitchTime += Random.Range(5f, 30f);
                glitchPower = Random.Range(0.8f, 3f) * glitchedFor * 0.01f;
                enableEffect(Random.Range(0, 4), Random.Range(0f, glitchedFor) > 3f);
            }
            unsyncEffect.speed = Mathf.Sin(glitchTime) * glitchPower;
            vramEffect.shift = Mathf.Cos(glitchTime * 3.1f) * (4f + Mathf.Tan(glitchTime) + glitchPower);
        }else
        {
            if (effectsEnabled)
            {
                effectsEnabled = false;
                unsyncEffect.enabled = false;
                bleedingColorEffect.enabled = false;
                vramEffect.enabled = false;
                tintEffect.enabled = false;
                bwEffect.enabled = false;
                glitchedFor = 0f;
            }
        }
	}
}
