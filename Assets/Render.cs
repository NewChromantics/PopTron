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

	const int DIR_DEAD	= 4;

	public void UpdateMapShader()
	{
		var game = GameObject.FindObjectOfType<Game>();
		var map = GameObject.FindObjectOfType<Map>();

		MapShader.SetInt("Width", map.Width );
		MapShader.SetInt("Height", map.Height );
		
		var MapTiles = new List<float>();
		var GameTiles = new List<float>();
		var PlayerDirs = new List<float>();

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

			MapTiles.Add( (float)MapTile );
			GameTiles.Add( (float)GameTile );
		}

		for ( int i=0;	i<game.Players.Count;	i++ )
		{
			var Player = game.Players[i];
			if ( Player.Alive )
				PlayerDirs.Add( (float)Player.Direction );
			else
				PlayerDirs.Add( DIR_DEAD );
		}

		MapShader.SetFloatArray("MapTiles", MapTiles );
		MapShader.SetFloatArray("GameTiles", GameTiles );
		MapShader.SetFloatArray("PlayerDirs", PlayerDirs );
		

		UnityEditor.SceneView.RepaintAll();
	}
}
