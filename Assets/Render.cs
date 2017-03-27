using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Render : MonoBehaviour {

	[System.Serializable]
	public class TickAnimation
	{
		public int				Time;		//	Time out of Duration in ticks
		public int				Duration;	//	life in ticks
		public PopperMan.Tile	Tile;
		public int2				xy;
	}
		
	public Material	MapShader;

	[InspectorButton("UpdateMapShader")]
	public bool		_UpdateMapShader;
	const int DIR_DEAD	= 4;

	//[System.NonSerialized]
	public Texture2D			_MapTiles;
	//[System.NonSerialized]
	public Texture2D			_GameTiles;


	[Range(0,10)]
	public float				ShakeDurationSecs = 3;
	public AnimationCurve		ExplodeShakeX;
	public AnimationCurve		ExplodeShakeY;

	[InspectorButton("ShakeScreen")]
	public bool		_ShakeScreen;
	float			ShakeStartTime = -1;
	float			ShakeTime {
		get {
			if (ShakeStartTime < 0)
				return 0;
			
			if (!Application.isPlaying) {
				return Mathf.Clamp01 (ShakeStartTime / ShakeDurationSecs);
			}
			return Mathf.Clamp01 ((Time.time - ShakeStartTime) / ShakeDurationSecs);
		}
	}




	public Vector2 GetCanvasTileUv(int2 xy)
	{
		var map = GameObject.FindObjectOfType<Map>();
		var uv = new Vector2();
		uv.x = xy.x / (float)map.Width;
		uv.y = xy.y / (float)map.Height;
		return uv;
	}

	void Update()
	{
		var game = GameObject.FindObjectOfType<Game>();
		MapShader.SetFloat("FrameDelta", game.FrameDelta );
		MapShader.SetFloat("Frame", game.Frame );
		
		UpdateMapShader();
	}

	void EditorUpdateShake()
	{
		if (!Application.isPlaying) {
			ShakeStartTime += 1.0f / 60;
			if (ShakeTime < 1) {
				Debug.Log ("ShakeTime = " + ShakeTime );
				UpdateMapShader ();
			}
		}
	}

	public void ShakeScreen()
	{
		#if UNITY_EDITOR
		if (!Application.isPlaying) 
		{
			Debug.Log ("Reset shake");
			ShakeStartTime = 0;
			//	force editor to update every N ms
			UnityEditor.EditorApplication.update += EditorUpdateShake;
		}
		else 
		#endif
		{
			ShakeStartTime = Time.time;
		}
	}


	static Texture2D		ResizeTexture(Texture2D OldTexture,int Width,int Height)
	{
		//	POW2 sizes to avoid sampling headaches later plz
		Width = Mathf.IsPowerOfTwo(Width) ? Width : Mathf.NextPowerOfTwo(Width);
		Height = Mathf.IsPowerOfTwo(Height) ? Height : Mathf.NextPowerOfTwo(Height);

		//	already fine
		if ( OldTexture != null )
		{
			if ( OldTexture.width >= Width && OldTexture.height >= Height )
				return OldTexture;
		}

		var NewTexture = new Texture2D( Width, Height, TextureFormat.RGB24, false );
		NewTexture.filterMode = FilterMode.Point;

		//	copy old data
		if ( OldTexture != null )
		{
			var OldPixels = OldTexture.GetPixels32();
			var NewPixels = NewTexture.GetPixels32();
			for ( int y=0;	y<Mathf.Min(OldTexture.height,NewTexture.height);	y++ )
			{
				for ( int x=0;	x<Mathf.Min(OldTexture.width,NewTexture.width);	x++ )
				{
					int o = (y * OldTexture.width) + x;
					int n = (y * NewTexture.width) + x;
					NewPixels[n] = OldPixels[o];
				}
			}
			NewTexture.SetPixels32 (NewPixels);
			NewTexture.Apply ();
		}
		return NewTexture;
	}



	Texture2D	MapTiles
	{
		get
		{
			_MapTiles.Apply ();
			return _MapTiles;
		}
	}

	Texture2D	GameTiles
	{
		get
		{
			_GameTiles.Apply ();
			return _GameTiles;
		}
	}

	Color32[] _MapTilePixels;

	Color32[] GetMapTilePixels(int Width,int Height)
	{
		_MapTiles = ResizeTexture( _MapTiles, Width, Height );
		if (_MapTilePixels!=null && _MapTilePixels.Length < Width * Height)
			_MapTilePixels = null;
		if (_MapTilePixels == null)
			_MapTilePixels = _MapTiles.GetPixels32 ();
		return _MapTilePixels;
	}

	Color32[] _GameTilePixels;

	Color32[] GetGameTilePixels(int Width,int Height)
	{
		_GameTiles = ResizeTexture( _GameTiles, Width, Height );
		if (_GameTilePixels != null && _GameTilePixels.Length < Width * Height)
			_GameTilePixels = null;
		if (_GameTilePixels == null)
			_GameTilePixels = _GameTiles.GetPixels32 ();
		return _GameTilePixels;
	}


	public void UpdateMapShader()
	{
		var game = GameObject.FindObjectOfType<Game>();
		var map = GameObject.FindObjectOfType<Map>();

		MapShader.SetInt("Width", map.Width );
		MapShader.SetInt("Height", map.Height );
		
		var PlayerDirs = new List<float>();

		var MapPixels = GetMapTilePixels (map.Width, map.Height);
		var GamePixels = GetGameTilePixels (map.Width, map.Height);

		var TempColour = new Color32 (0, 0, 0, 1);
		System.Action<int2,PopperMan.Tile> SetMapTile = (int2 xy, PopperMan.Tile Tile) => {
			TempColour.r = (byte)Tile;
			MapPixels [(xy.y * _MapTiles.width) + xy.x] = TempColour;
		};
		System.Action<int2,PopperMan.Tile> SetGameTile = (int2 xy, PopperMan.Tile Tile) => {
			TempColour.r = (byte)Tile;
			GamePixels [(xy.y * _GameTiles.width) + xy.x] = TempColour;
		};


		map.ForEachTile ((Tile, xy) => {
			SetMapTile( xy, Tile );
		}
		);

		{
			var xy = new int2 (0, 0);
			for (xy.y = 0;	xy.y < map.Height;	xy.y++) {
				for (xy.x = 0;	xy.x < map.Width;	xy.x++) {
					SetGameTile (xy, PopperMan.Tile.None);
				}
			}

			foreach (var Player in game.Players) {
				var PlayerIndex = game.Players.IndexOf (Player);
				var GameTile = PopperMan.GetPlayerTile (PlayerIndex);
				SetGameTile (Player.xy, GameTile);
			}
		}

		/*
		ForEachTile
		for ( int i=0;	i<map.Width*map.Height;	i++ )
		{
			var xy = map.GetMapXy(i);
			var MapTile = map[xy];
			var GameTile = PopperMan.Tile.None;
			
			{
				var Player = game.GetPlayerAt(xy);
				if ( Player )
				{
					var PlayerIndex = game.Players.IndexOf(Player);
					GameTile = PopperMan.GetPlayerTile(PlayerIndex);
				}
			}

			SetMapTile (xy, MapTile);
			SetGameTile (xy, GameTile);
		}
*/
		for ( int i=0;	i<game.Players.Count;	i++ )
		{
			var Player = game.Players[i];
			if ( Player.Alive )
				PlayerDirs.Add( (float)Player.Direction );
			else
				PlayerDirs.Add( DIR_DEAD );
		}

		_MapTiles.SetPixels32( MapPixels );
		_GameTiles.SetPixels32( GamePixels );
		_MapTiles.Apply ();
		_GameTiles.Apply ();

		MapShader.SetTexture("MapTiles", _MapTiles );
		MapShader.SetTexture("GameTiles", _GameTiles );
		MapShader.SetFloatArray("PlayerDirs", PlayerDirs );

		var ShakeOffset = new Vector4 (ExplodeShakeX.Evaluate (ShakeTime), ExplodeShakeX.Evaluate (ShakeTime), 0, 0);
		MapShader.SetVector ("ShakeOffset", ShakeOffset);

		#if UNITY_EDITOR
		{
			if (!Application.isPlaying) {
				UnityEditor.SceneView.RepaintAll ();
			}
		}
		#endif
	}
}
