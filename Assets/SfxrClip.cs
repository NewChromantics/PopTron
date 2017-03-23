using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxrClip : MonoBehaviour {

	[Multiline(4)]
	public string 		SfxSample = "3,.5,,.2601,.5898,.3845,.3,.0526,,-.0026,,,,,,,,,,,,,,,,1,,,,,,";
	public SfxrSynth			Synth;

	[InspectorButton("Play")]
	public bool _Play;

	public void Play()
	{
		Synth = new SfxrSynth ();
		Synth.parameters.SetSettingsString (SfxSample);
		Synth.Play ();
	}
}
