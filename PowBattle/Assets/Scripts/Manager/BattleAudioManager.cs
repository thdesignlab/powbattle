using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class BattleAudioManager : AudioManager
{
    protected override void Init()
    {
        base.Init();
        audioSource.outputAudioMixerGroup = Common.Var.audioMixer.FindMatchingGroups(Common.CO.AUDIO_MIXER_BATTLE)[0];
        audioSource.spatialBlend = 1.0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 5.0f;
        audioSource.maxDistance = 50.0f;
    }
}
