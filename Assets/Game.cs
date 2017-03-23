using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class UnityEvent_Player : UnityEngine.Events.UnityEvent <Player> {}


[System.Serializable]
public class UnityEvent_int2 : UnityEngine.Events.UnityEvent <int2> {}


public class Game : MonoBehaviour {

	public string	ResetButtonName = "Start";

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
	public UnityEvent_Player				OnPlayerExplode;
	public UnityEvent_Player				OnPlayerHeadbutt;
	public UnityEngine.Events.UnityEvent	OnGameFinished;
	public UnityEngine.Events.UnityEvent	OnGameStart;
	public UnityEngine.Events.UnityEvent	OnTickEnd;


	
	private void Update()
	{
		if (Input.GetButtonDown (ResetButtonName)) {
			var ThisScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
			UnityEngine.SceneManagement.SceneManager.LoadScene (ThisScene.name);
			return;
		}
		
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

		//	pre-calc all player new-head positions for head-head collision
		var OldPlayerHeads = new int2[Players.Count];
		var NewPlayerHeads = new int2[Players.Count];
		for (int p = 0;	p < Players.Count;	p++) {
			var player = Players[p];
			var NewPos = player.xy;
			NewPos.x += PopperMan.GetDelta(player.Direction).x;
			NewPos.y += PopperMan.GetDelta(player.Direction).y;
			OldPlayerHeads [p] = player.xy;
			NewPlayerHeads [p] = NewPos;
		}

		//	find map crashes
		var MapCollision = new bool[Players.Count];
		for (int p = 0;	p < Players.Count;	p++) {
			var MapTile = map [NewPlayerHeads[p]];
			var Crash = (MapTile != PopperMan.Tile.Floor);
			MapCollision[p] = Crash;

			//	the current/last frame head hasn't been written to the map, but we still need to collide with it
			for (int q = 0;	q < Players.Count;	q++) {
				if (p == q)
					continue;
				if (NewPlayerHeads [p] == OldPlayerHeads [q])
					MapCollision [p] = true;
			}
		}

		//	find head-head coliisoins
		var HeadCollision = new bool[Players.Count];


		for (int p = 0;	p < Players.Count;	p++) {
			for (int q = p + 1;	q < Players.Count;	q++) {
				if (!Players [p].Alive)
					continue;
				if (!Players [q].Alive)
					continue;
				var Collision = NewPlayerHeads [p] == NewPlayerHeads [q];
				if (Collision)
					Debug.Log ("Collision between " + p + " and " + q);
				HeadCollision[p] |= Collision;
				HeadCollision[q] |= Collision;
			}
		}


		//	kill players
		for (int p = 0;	p < Players.Count;	p++) {
			var player = Players [p];
			if (!player.Alive)
				continue;

			var Collision = HeadCollision [p] || MapCollision [p];
			if (!Collision)
				continue;
			
			player.Alive = false;

			if (HeadCollision [p])
				OnPlayerHeadbutt.Invoke (player);
			else
				OnPlayerExplode.Invoke (player);

			//	erase from map
			var PlayerTile = PopperMan.GetPlayerTile (p);
			map.ForEachTile ((Tile, xy) => {
				if (Tile != PlayerTile)
					return;
				map [xy] = PopperMan.Tile.Floor;
			}
			);
		}

		//	move live players
		for (int p = 0;	p < Players.Count;	p++) {
			var player = Players [p];

			if (player.Alive) {
				//	burn old pos to map
				try
				{
					map [player.xy] = PopperMan.GetPlayerTile (p);
				}
				catch{};
				player.xy = NewPlayerHeads [p];
			}
		}

		for ( int p=0;	p<Players.Count;	p++ )
		{
			var player = Players[p];
			player.ClearInput ();
		}
	
		OnTickEnd.Invoke ();
	}
}
