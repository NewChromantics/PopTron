﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnityEvent_Bomb : UnityEngine.Events.UnityEvent <Game.Bomb> {}

[System.Serializable]
public class UnityEvent_Player : UnityEngine.Events.UnityEvent <Player> {}


[System.Serializable]
public class UnityEvent_int2 : UnityEngine.Events.UnityEvent <int2> {}


public class Game : MonoBehaviour {

	public class Bomb
	{
		public int		x {	get {return xy.x; } }
		public int		y {	get {return xy.y; } }
		public int2		xy;
		public int		StartFrame;
		public int		Duration;
		public Player	Player;
		public int		Radius;

		//	store flames upon explosion for anim
		public List<int2>	Flames = new List<int2>();
	};

	[InspectorButton("Tick")]
	public bool	_Tick;

	[System.NonSerialized]
	public int Frame = 0;


	[Range(1,60)]
	public int		TicksPerSec = 5;
	float			TickCountdown = 0;
	float			TickMs {	get {	return 1000 / (float)TicksPerSec; } }
	float			TickSecs {	get {	return TickMs / 1000.0f; } }
//	the time (0...1) remaining before the next tick
	public float	FrameDelta { get	{	return (TickSecs-TickCountdown) / TickSecs;	}	}


	public List<Player>	_Players;
	public List<Bomb>	Bombs = new List<Bomb>();
	public List<Player>	Players
	{
		get
		{
			if ( _Players == null || _Players.Count == 0 )
				_Players = new List<Player>( GameObject.FindObjectsOfType<Player>() );
			return _Players;
		}
	}

	[Header("Game events - eg sound")]
	public UnityEvent_Bomb					OnBombPlaced;
	public UnityEvent_Bomb					OnBombExplode;
	public UnityEvent_Player				OnPlayerDeathExplode;
	public UnityEvent_Player				OnPlayerJoin;
	public UnityEngine.Events.UnityEvent	OnGameFinished;
	public UnityEngine.Events.UnityEvent	OnGameStart;
	public UnityEngine.Events.UnityEvent	OnTickEnd;
	public UnityEvent_int2					OnWallDestroyed;
	

	
	private void Update()
	{
		TickCountdown -= Time.deltaTime;
		if ( TickCountdown < 0 )
		{
			Tick();
			TickCountdown = TickSecs;
		}
	}

	
	public Player GetPlayerAt(int2 xy)
	{
		foreach (var Player in Players) {
			if (Player.xy == xy)
				return Player;
		}
		return null;
	}


	PopperMan.Tile GetMapTileAt(int2 xy)
	{
		var map = GameObject.FindObjectOfType<Map>();
		return map [xy];
	}

	
	bool CanPlayerMoveTo(int2 xy,Player self)
	{
		return true;
	}




	public void Tick()
	{
		if ( Frame == 0 )
			OnGameStart.Invoke();

		Frame++;

		//	move each player
		var map = GameObject.FindObjectOfType<Map>();

		//foreach ( var player in Players )
		for ( int p=0;	p<Players.Count;	p++ )
		{
			var player = Players[p];

			if ( player.Alive )
			{
				var OldPos = player.xy;
				var NewPos = player.xy;
				NewPos.x += PopperMan.GetDelta(player.Direction).x;
				NewPos.y += PopperMan.GetDelta(player.Direction).y;

				//	check for crash
				bool Crash = false;

				if ( Crash )
				{

				}
				else
				{
					//	burn old pos to map
					map[OldPos] = PopperMan.GetPlayerTile(p);
					player.xy = NewPos;
				}
				//	get forward pos
				//	did we crash into map
				//	did we crash into a player body
				//	did we crash into a player head
				//	move forward
			}
			
			player.ClearInput ();
		}
	
		OnTickEnd.Invoke ();
	}
}
