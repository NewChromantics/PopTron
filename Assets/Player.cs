using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class UnityEvent_String : UnityEngine.Events.UnityEvent <string> {}


[ExecuteInEditMode]
public class Player : MonoBehaviour {

	public string	JoystickAxisName = "Joystick 1 Horizontal";

	public PopperMan.Direction	Direction = PopperMan.Direction.Up;

	[Range(0,40)]
	public int		x = 1;

	[Range(0,40)]
	public int		y = 1;

	public int2 xy
	{
		get
		{
			return new int2(x,y);
		}
	}


	public bool		Alive = false;






	public void ClearInput()
	{
		/*
		//	work out if we let-go in a frame
		Input_Direction = PopperMan.Direction.None;
		SetDirectionIfKey( PopperMan.Direction.Up, PopperMan.NesPadJoystickButton.Up );
		SetDirectionIfKey( PopperMan.Direction.Down, PopperMan.NesPadJoystickButton.Down );
		SetDirectionIfKey( PopperMan.Direction.Left, PopperMan.NesPadJoystickButton.Left );
		SetDirectionIfKey( PopperMan.Direction.Right, PopperMan.NesPadJoystickButton.Right );
		TickStart_Direction = Input_Direction;
		Input_Direction = PopperMan.Direction.None;
		*/
	}

	void Start()
	{
	}

	void Update()
	{
		//	we OR the inputs, as they're only used on a tick, we store it until
		var Reading = Input.GetAxisRaw(JoystickAxisName);
		if ( Reading < 0 )
		{
			if ( Direction == PopperMan.Direction.Up )			Direction = PopperMan.Direction.Left;
			else if ( Direction == PopperMan.Direction.Left )	Direction = PopperMan.Direction.Down;
			else if ( Direction == PopperMan.Direction.Down )	Direction = PopperMan.Direction.Right;
			else if ( Direction == PopperMan.Direction.Right )	Direction = PopperMan.Direction.Up;
		}
		else if ( Reading > 0 )
		{
			if ( Direction == PopperMan.Direction.Up )			Direction = PopperMan.Direction.Right;
			else if ( Direction == PopperMan.Direction.Left )	Direction = PopperMan.Direction.Up;
			else if ( Direction == PopperMan.Direction.Down )	Direction = PopperMan.Direction.Left;
			else if ( Direction == PopperMan.Direction.Right )	Direction = PopperMan.Direction.Down;
		}

	}

	public void Move(System.Func<int2,bool,Player,bool> CanMoveTo)
	{
		/*
		if ( Input_Direction == PopperMan.Direction.None )
			return;

		var NewPos = PopperMan.Move( new int2(x,y), Input_Direction );
		if ( !CanMoveTo( NewPos, this.Alive, this ) )
		{
			OnBump.Invoke();
			return;
		}

		x = NewPos.x;
		y = NewPos.y;
		OnPlayerMoved.Invoke();
		*/
	}

	

}
