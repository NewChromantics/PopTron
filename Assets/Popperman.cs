using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PopperMan
{
	//	same as shader
	const int TILE_INVALID	= -1;
	const int TILE_NONE		= 0;
	const int TILE_FLOOR	= 1;
	const int TILE_WALL		= 2;
	const int TILE_PLAYER0	= 3;
	const int TILE_PLAYER1	= 4;
	const int TILE_PLAYER2	= 5;
	const int TILE_PLAYER3	= 6;


	public enum Tile
	{
		//	map tiles
		Floor		= TILE_FLOOR,
		Wall		= TILE_WALL,

		//	meta tiles
		OutOfBounds	= TILE_INVALID,
		Invalid		= TILE_INVALID,
		None		= TILE_NONE,

		//	game tiles
		Player0		= TILE_PLAYER0,
		Player1		= TILE_PLAYER1,
		Player2		= TILE_PLAYER2,
		Player3		= TILE_PLAYER3,
		
	}

	public static Tile	GetPlayerTile(int Index) {	return (Tile)( (int)TILE_PLAYER0 + Index ); }

	public enum Direction
	{
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3
	}
	
	public static int2	GetDelta(Direction direction)
	{
		switch ( direction )
		{
			case Direction.Up:		return new int2( 0, -1 );
			case Direction.Down:	return new int2( 0, 1 );
			case Direction.Left:	return new int2( -1, 0 );
			case Direction.Right:	return new int2( 1, 0 );
			default:				throw new System.Exception("Invalid direction " + direction);
		}
	}

	public static int2	Move(int2 xy,Direction direction)
	{
		var Delta = GetDelta( direction );
		xy.x += Delta.x;
		xy.y += Delta.y;
		return xy;
	}



}

//	generic funcs!
public class Pop
{
	static public void AddUnique<T>(List<T> Array,T Value) where T : class
	{
		if ( Array.Exists( (v) => { return v==Value;	}	) )
			return;

		Array.Add( Value );
	}

}



public class int2
{
	public int x;
	public int y;

	public int2(int _x,int _y)
	{
		x = _x;
		y = _y;
	}

	public static bool operator== (int2 a, int2 b)
    {
        return ( a.x == b.x && a.y == b.y );
    }

	public static bool operator!= (int2 a, int2 b)
    {
         return !(a == b);
    }
	
    public override bool Equals(object b)
    {
         return b.GetType() == GetType() && (this == (int2)b );
    }
	

}


