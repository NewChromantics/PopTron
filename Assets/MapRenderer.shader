Shader "PoppperMan/MapRenderer"
{
	Properties
	{
		FrameDelta("FrameDelta", Range(0,1) ) = 0
		Width("Width",Range(1,100) ) = 20
		Height("Height",Range(1,100) ) = 20
		ShakeScalarPx("ShakeScalarPx", Range(0,100) ) = 1

		TileColour_Floor("TileColour_Floor",COLOR) = (0,0,0,1)
		TileColour_Wall("TileColour_Wall",COLOR) = (1,1,1,1)
		TileColour_Player0("TileColour_Player0",COLOR) = (1,0,0,1)
		TileColour_Player1("TileColour_Player1",COLOR) = (0,1,1,1)
		TileColour_Player2("TileColour_Player2",COLOR) = (1,1,0,1)
		TileColour_Player3("TileColour_Player3",COLOR) = (1,0,1,1)

		MapTiles("MapTiles", 2D ) = "black"
		GameTiles("GameTiles", 2D ) = "black"

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

		
			#define TILE_INVALID -1
		#define TILE_NONE		0
		#define TILE_FLOOR		1
		#define TILE_WALL		2
		#define TILE_PLAYER0	3
		#define TILE_PLAYER1	4
		#define TILE_PLAYER2	5
		#define TILE_PLAYER3	6
		#define TILE_COUNT		7

		#define TileColour_Invalid float4(0,0,1,0)
		#define TileColour_None float4(0,1,0,0)
			float4 TileColour_Floor;
			float4 TileColour_Wall;
			float4 TileColour_Player0;
			float4 TileColour_Player1;
			float4 TileColour_Player2;
			float4 TileColour_Player3;

			int Width;
			int Height;
			float FrameDelta;
			float Frame;
			float2 ShakeOffset;
			float ShakeScalarPx;


		#define MAX_PLAYERS	4

		#define DIR_UP		0
		#define DIR_DOWN	1
		#define DIR_LEFT	2
		#define DIR_RIGHT	3
		#define DIR_DEAD	4
			float PlayerDirs[MAX_PLAYERS];

			sampler2D MapTiles;
			float4 MapTiles_TexelSize;
			sampler2D GameTiles;
			float4 GameTiles_TexelSize;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv.y = 1 - o.uv.y;
				return o;
			}

			float4 GetMapTileColour(int2 Tilexy,float2 Tileuv,int Tile)
			{
				//	gr: switch not supported on "es 2" (whichever shader version that is)
				float4 TileColour_x[TILE_COUNT+1];
				TileColour_x[TILE_NONE]		= TileColour_None;
				TileColour_x[TILE_FLOOR]	= TileColour_Floor;
				TileColour_x[TILE_WALL]		= TileColour_Wall;
				TileColour_x[TILE_PLAYER0]	= TileColour_Player0;
				TileColour_x[TILE_PLAYER1]	= TileColour_Player1;
				TileColour_x[TILE_PLAYER2]	= TileColour_Player2;
				TileColour_x[TILE_PLAYER3]	= TileColour_Player3;
				TileColour_x[TILE_COUNT]	= TileColour_Invalid;

				if ( Tile < 0 || Tile >= TILE_COUNT )
					Tile = TILE_COUNT;

				return TileColour_x[Tile];
			}

			float GetArrowUpMask(float2 uv)
			{
				float Minx = uv.y;
				float x = abs( uv.x - 0.5f ) * 2;
				return (x>Minx) ? 0 : 1;
			}

			float GetArrowDownMask(float2 uv)
			{
				float Minx = 1 - uv.y;
				float x = abs( uv.x - 0.5f ) * 2;
				return (x>Minx) ? 0 : 1;
			}

			float GetArrowLeftMask(float2 uv)
			{
				float Minx = uv.x;
				float x = abs( uv.y - 0.5f ) * 2;
				return (x>Minx) ? 0 : 1;
			}

			float GetArrowRightMask(float2 uv)
			{
				float Minx = 1 - uv.x;
				float x = abs( uv.y - 0.5f ) * 2;
				return (x>Minx) ? 0 : 1;
			}

			float GetCircleMask(float2 uv,float Radius)
			{
				uv -= 0.5f;
				uv *= 2;
				float Rad = length( uv );
				return (Rad > Radius) ? 0 : 1;
			}

			float4 GetArrowColour(float2 uv,float4 Colour,int Direction)
			{
				float Mask = 0;
				if ( Direction == DIR_UP )			Mask = GetArrowUpMask( uv );
				else if ( Direction == DIR_DOWN )	Mask = GetArrowDownMask( uv );
				else if ( Direction == DIR_LEFT )	Mask = GetArrowLeftMask( uv );
				else if ( Direction == DIR_RIGHT )	Mask = GetArrowRightMask( uv );
				else
				{
					Mask = GetCircleMask( uv, 0.5f );
				}
				return lerp( TileColour_Invalid, Colour, Mask );
			}

			float4 GetGameTileColour(int2 Tilexy,float2 Tileuv,int Tile)
			{
				//	gr: switch not supported on "es 2" (whichever shader version that is)
				float4 TileColour_x[TILE_COUNT+1];
				TileColour_x[TILE_NONE]		= TileColour_None;
				TileColour_x[TILE_FLOOR]	= TileColour_Floor;
				TileColour_x[TILE_WALL]		= TileColour_Wall;
				TileColour_x[TILE_PLAYER0]	= GetArrowColour( Tileuv, TileColour_Player0, PlayerDirs[0] );
				TileColour_x[TILE_PLAYER1]	= GetArrowColour( Tileuv, TileColour_Player1, PlayerDirs[1] );
				TileColour_x[TILE_PLAYER2]	= GetArrowColour( Tileuv, TileColour_Player2, PlayerDirs[2] );
				TileColour_x[TILE_PLAYER3]	= GetArrowColour( Tileuv, TileColour_Player3, PlayerDirs[3] );
				TileColour_x[TILE_COUNT]	= TileColour_Invalid;

				if ( Tile < 0 || Tile >= TILE_COUNT )
					Tile = TILE_COUNT;

				return TileColour_x[Tile];
			
			}

			float3 BlendColour(float3 Bottom,float4 Top,float AlphaMult)
			{
				float3 Rgb = lerp( Bottom.xyz, Top.xyz, Top.w * AlphaMult );
				return Rgb;
			}

			int GetMapTile(int x,int y)
			{
				float2 TileUv = float2(x,y) * MapTiles_TexelSize.xy;
				return tex2D( MapTiles, TileUv ).x * 255.0f;
			}

			int GetGameTile(int x,int y)
			{
				float2 TileUv = float2(x,y) * GameTiles_TexelSize.xy;
				return tex2D( GameTiles, TileUv ).x * 255.0f;
			}

			float4 GetBackgroundColour(float2 uv)
			{
				return float4(0,0,0,1);
			}

			fixed4 frag (v2f Frag) : SV_Target
			{
				float2 ShakeOffsetUv = ShakeScalarPx * MapTiles_TexelSize.xy * ShakeOffset;
				float2 uv = (Frag.uv+ShakeOffsetUv) * float2(Width,Height);
				int x = floor( uv.x );
				int y = floor( uv.y );
				if ( x < 0 || x >= Width || y < 0 || y >= Height )
					return GetBackgroundColour( uv );
				uv = frac(uv);
				int i = x + (y*Width);

				int MapTile = GetMapTile( x, y );
				int GameTile = GetGameTile( x, y );

				float4 MapColour = GetMapTileColour( int2(x,y), uv, MapTile );
				float4 GameColour = GetGameTileColour( int2(x,y), uv, GameTile );
				float4 Colour = lerp( MapColour, GameColour, GameColour.w );

				//	debug framedelta
				//Colour = lerp( Colour, float3(uv,0), FrameDelta );

				return float4( Colour.xyz, 1 );
			}
			ENDCG
		}
	}
}
