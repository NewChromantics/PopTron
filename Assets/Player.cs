using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class UnityEvent_String : UnityEngine.Events.UnityEvent <string> {}


[ExecuteInEditMode]
public class Player : MonoBehaviour {

	public string	JoystickAxisName_Windows = "Joystick 1 Horizontal";
	public string	JoystickAxisName_Osx = "Joystick 1 Horizontal";
	public KeyCode	LeftKey = KeyCode.Q;
	public KeyCode	RightKey = KeyCode.W;

	public PopperMan.Direction	Direction = PopperMan.Direction.Up;
	public PopperMan.Direction	TickStart_Direction = PopperMan.Direction.Up;

	[Range(0,200)]
	public int		x = 1;

	[Range(0,200)]
	public int		y = 1;

	public int2 xy
	{
		get
		{
			return new int2(x,y);
		}
		set
		{
			x = value.x;
			y = value.y;
		}
	}


	public bool		Alive = false;

	int LastAxisReading = 0;
	public string	JoystickAxisName
	{
		get
		{
			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return JoystickAxisName_Osx;
			#else
			return JoystickAxisName_Windows;
			#endif
		}
	}






	public void ClearInput()
	{
		TickStart_Direction = Direction;
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
		//	we OR the inputs, as they're only used on a tick, we store it until frame is done
		var NewAxisReading = (int)Input.GetAxisRaw(JoystickAxisName);
		bool LeftDown = (NewAxisReading < 0) && (LastAxisReading >= 0);
		bool RightDown = (NewAxisReading > 0) && (LastAxisReading <= 0);
		LastAxisReading = NewAxisReading;

		if ( LeftDown || Input.GetKeyDown(LeftKey) )
		{
			if ( TickStart_Direction == PopperMan.Direction.Up )			Direction = PopperMan.Direction.Left;
			else if ( TickStart_Direction == PopperMan.Direction.Left )	Direction = PopperMan.Direction.Down;
			else if ( TickStart_Direction == PopperMan.Direction.Down )	Direction = PopperMan.Direction.Right;
			else if ( TickStart_Direction == PopperMan.Direction.Right )	Direction = PopperMan.Direction.Up;
		}
			else if ( RightDown || Input.GetKeyDown(RightKey) )
		{
			if ( TickStart_Direction == PopperMan.Direction.Up )			Direction = PopperMan.Direction.Right;
			else if ( TickStart_Direction == PopperMan.Direction.Left )	Direction = PopperMan.Direction.Up;
			else if ( TickStart_Direction == PopperMan.Direction.Down )	Direction = PopperMan.Direction.Left;
			else if ( TickStart_Direction == PopperMan.Direction.Right )	Direction = PopperMan.Direction.Down;
		}

	}

	

}
