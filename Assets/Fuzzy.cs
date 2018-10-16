using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fuzzy : MonoBehaviour 
{
	// Struct used to store the information of a triangle
	public struct triangle
	{
		public Vector2 point_1;	// Bottom_Left
		public Vector2 point_2;	// Top
		public Vector2 point_3;	// Bottom_Right

		public float area;

		public void Triangle_Area()
		{
			// Area = 1/2(Base*Height)
			area = ((point_3.x - point_1.x) * (point_2.y - point_1.y)) / 2;
		}
	};

	// Struct used tore the information of a trapezoid and calculate the area and center of mass
	public struct trapezoid
	{
		public Vector2 top_left;
		public Vector2 top_right;
		public Vector2 bottom_left;
		public Vector2 bottom_right;

		public float top, bottom, height, area, com;

		// Calculates the values for the top edge, bottom edge and height
		public void Trapezoid_TBH ()
		{
			area = 0;
			com = 0;
			top = top_right.x - top_left.x;
			bottom = bottom_right.x - bottom_left.x;
			height = top_left.y - bottom_left.y;
		}

		// Calculates the Area and Center of mass
		public void Trapezoid_Area_Com ()
		{
			//print("1");
			//		 ___
			//		/___\
			if (top_left.x != bottom_left.x && top_right.x != bottom_right.x) 
			{
				// Area of triangle on left
				float tla = ((top_left.x - bottom_left.x) * height) / 2;
				// Center of mass of left triangle
				float tlc = bottom_left.x + ((2 * (top_left.x - bottom_left.x))/ 3);
				// Combined 
				float tl = tla * tlc;

				// Area of triangle on right
				float tra = ((bottom_right.x - top_right.x) * height) / 2; 
				// Center of mass of right triangle
				float trc = top_right.x + ((1 * (bottom_right.x - top_right.x)) / 3);
				// Combined
				float tr = tra * trc;
	
				// Area of square in the middle
				float sqa = (top) * height;
				// Center of mass of square
				float sqc = top_left.x + (top / 2);
				// Combined
				float sq = sqa * sqc;

				// Total area
				area = tla + tra + sqa;

				// Total Combined
				com = tl + tr + sq;
			}
			//		 __
			//		|___\
			else if (top_left.x == bottom_left.x && top_right.x != bottom_right.x) // triangle on right
			{
				// Area of square - bxh
				float sqa = top * height;
				// Center of mass of square
				float sqc = bottom_left.x + (top / 2);
				// Combined
				float sq = sqa * sqc;

				// Area of triangle - 1/2(bxh)
				float ta = ((bottom - top) * height) / 2;
				// Center of mass of triangle
				float tc = top_right.x + ((1 * (bottom_right.x - top_right.x)) / 3);
				// Combined
				float t = ta * tc;

				// Total area
				area = sqa + ta;

				// Total Combined
				//com = sq + t;
				com = (sqa*sqc) + (ta*tc);
			}
			//		 __
			//		/__|
			else if (top_left.x != bottom_left.x && top_right.x == bottom_right.x) // triangle on left
			{
				// Area of square - bxh
				float sqa = top * height;
				// Center of mass of square
				float sqc = top_left.x + (top / 2);
				// Combined
				float sq = sqa * sqc;

				// Area of triangle - 1/2(bxh)
				float ta = ((bottom - top) * height) /2;
				// Center of mass of triangle
				float tc = bottom_left.x + ((2 * (top_left.x - bottom_left.x)) / 3);
				// Combined
				float t = ta * tc;	

				// Total area
				area = sqa + ta;
				// Total Combined
				//com = sq + t;
				com = (sqa*sqc) + (ta*tc);
			}
		}
	};

	// Struct to contain the information for the output graph (5 triangles)
	public struct output_graph
	{
		public triangle v_left;
		public triangle left;
		public triangle zero;
		public triangle right;
		public triangle v_right;
	};

	// Struct to contain the information for both the input graphs (3 triangles)
	public struct input_graph
	{
		public triangle left;
		public triangle zero;
		public triangle right;
	};

	// Struct to contain all results from the rules etc.
	public struct rule_results
	{
		// Y coord of intersection between line and graph
		public float rule_1_y;
		public float rule_2_y;
		public float rule_3_y;
		public float rule_4_y;
		public float rule_5_y;
		public float rule_6_y;
		public float rule_7_y;
		public float rule_8_y;
		public float rule_9_y;

		// Final Y coord after comparing the Y coords of all membership functions in the output
		public float v_right;
		public float right;
		public float zero;
		public float left;
		public float v_left;

		// Trapezoid shapes created from the area beneath the line of the result
		public trapezoid vr;
		public trapezoid r;
		public trapezoid z;
		public trapezoid l;
		public trapezoid vl;
	};

	// Struct objects
	output_graph output_;// = new output_graph();
	input_graph distance_;//= new input_graph();
	input_graph velocity_;// = new input_graph();
	rule_results results_;// = new rule_results();

	// Floats used in calculations etc.
	float crisp_output;
	float distance_line;
	float velocity_line;
	float road_line;

	// The GameObjects used in the application
	public GameObject road;
	public GameObject car;

	// Bool to toggle manual input
	bool manual_input;
	public Canvas input_canvas;
	public GameObject output;

	public GameObject slider_1;
	public GameObject slider_2;
	public bool button_pressed = false;


	// Use this for initialization
	void Start () 
	{
		// Enable manual input to false
		manual_input = false;
		input_canvas.enabled = false;

		// Initialise the output graph
		output_.v_left.point_1 = new Vector2(-2, 0);
		output_.v_left.point_2 = new Vector2(-2, 1);
		output_.v_left.point_3 = new Vector2(-1.5f, 0);

		output_.left.point_1 = new Vector2(-1.5f, 0);
		output_.left.point_2 = new Vector2(-1, 1);
		output_.left.point_3 = new Vector2(-0.5f, 0);

		output_.zero.point_1 = new Vector2(-0.5f, 0);
		output_.zero.point_2 = new Vector2(0, 1);
		output_.zero.point_3 = new Vector2(0.5f, 0);

		output_.right.point_1 = new Vector2(0.5f, 0);
		output_.right.point_2 = new Vector2(1, 1);
		output_.right.point_3 = new Vector2(1.5f, 0);

		output_.v_right.point_1 = new Vector2(1.5f, 0);
		output_.v_right.point_2 = new Vector2(2, 1);
		output_.v_right.point_3 = new Vector2(2, 0);

		// Initialise the distance input graph
		distance_.left.point_1 = new Vector2(-2, 0);
		distance_.left.point_2 = new Vector2(-2, 1);
		distance_.left.point_3 = new Vector2(2, 0);

		distance_.zero.point_1 = new Vector2(-2, 0);
		distance_.zero.point_2 = new Vector2(0, 1);
		distance_.zero.point_3 = new Vector2(2, 0);

		distance_.right.point_1 = new Vector2(-2, 0);
		distance_.right.point_2 = new Vector2(2, 1);
		distance_.right.point_3 = new Vector2(2, 0);

		// Initialise the velocity input graph
		velocity_.left.point_1 = new Vector2(-2, 0);
		velocity_.left.point_2 = new Vector2(-2, 1);
		velocity_.left.point_3 = new Vector2(2, 0);

		velocity_.zero.point_1 = new Vector2(-2, 0);
		velocity_.zero.point_2 = new Vector2(0, 1);
		velocity_.zero.point_3 = new Vector2(2, 0);

		velocity_.right.point_1 = new Vector2(-2, 0);
		velocity_.right.point_2 = new Vector2(2, 1);
		velocity_.right.point_3 = new Vector2(2, 0);

		// Initialise floats to 0
		distance_line = 0;
		velocity_line = 0;
		crisp_output = 0;
		road_line = 0;
	}

	// Update is called once per frame
	void Update () 
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown (KeyCode.Space)) 
		{
			if (manual_input == false) 
			{
				manual_input = true;
				input_canvas.enabled = true;
			}
			else
			{
				manual_input = false;
				input_canvas.enabled = false;
			}
		}


		if (manual_input == false)
		{
			// Move racing line right
			if (Input.GetKey(KeyCode.RightArrow))
			{
				if (road_line + 0.1f <= 2)
				{
					road_line += 0.1f;
					road.transform.position = new Vector3(road_line*7.5f, road.transform.position.y, road.transform.position.z);
				}
				else 
				{
					road_line = 2;
					road.transform.position = new Vector3(road_line*7.5f, road.transform.position.y, road.transform.position.z);
				}
			}
			// Move racing line left
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				if (road_line - 0.01f >= -2)
				{
					road_line -= 0.1f;
					road.transform.position = new Vector3(road_line*7.5f, road.transform.position.y, road.transform.position.z);
				}
				else
				{
					road_line = -2;
					road.transform.position = new Vector3(road_line*7.5f, road.transform.position.y, road.transform.position.z);
				}
			}

			// Work out the distance between the car and the line
			distance_line = Mathf.Clamp((car.transform.position.x - road.transform.position.x)/ 15.0f, -2, 2);

			// Check Rules
			Generate_Rules();
			Compare_Results();
			Calculate_Output();

			// Move the car towards the line based on the output
			velocity_line += crisp_output * Time.deltaTime * 20.0f ;
			car.transform.Translate(velocity_line, 0, 0);
		}
		else
		{
			distance_line = slider_1.GetComponent<Slider>().value;
			velocity_line = slider_2.GetComponent<Slider>().value;

			if(button_pressed)
			{
				// Check Rules
				Generate_Rules();
				Compare_Results();
				Calculate_Output();

				// Move the car towards the line based on the output
				output.GetComponent<Text>().text = (crisp_output - (crisp_output % 0.001f)).ToString();

				distance_line = 0;
				velocity_line = 0;
				crisp_output = 0;
				road_line = 0;

				button_pressed = false;
			}
		}
	}

	public void PressButton()
	{
		button_pressed = !button_pressed;
	}

	float Intersection_PointY (Vector2 start, Vector2 end, float line_x)
	{
		// Gradient of the line
		float m = (end.y - start.y) / (end.x - start.x);

		// Y intersection of line
		float b = start.y - (m * start.x);

		// Y-coord of intersection point
		float y_coord = (m * line_x) + b;

		return y_coord;
	}

	float Intersection_PointX (Vector2 start, Vector2 end, float line_y)
	{
		// Gradient of the line
		float m = (end.y - start.y) / (end.x - start.x);

		// Y intersection of line
		float b = start.y - (m * start.x);

		// X-coord of intersection point
		float x_coord = (line_y - b) / m;

		return x_coord;
	}
		
	void Generate_Rules ()
	{
		// RULE 1:	LEFT-LEFT
		results_.rule_1_y = Mathf.Min(Intersection_PointY(distance_.left.point_2, distance_.left.point_3, distance_line), Intersection_PointY(velocity_.left.point_2, velocity_.left.point_3, velocity_line));
	
		// RULE 2:	LEFT-ZERO
		if (velocity_line >= 0) 
		{
			results_.rule_2_y = Mathf.Min (Intersection_PointY (distance_.left.point_2, distance_.left.point_3, distance_line), Intersection_PointY (velocity_.zero.point_2, velocity_.zero.point_3, velocity_line));
		}
		else 
		{
			results_.rule_2_y = Mathf.Min (Intersection_PointY (distance_.left.point_2, distance_.left.point_3, distance_line), Intersection_PointY (velocity_.zero.point_1, velocity_.zero.point_2, velocity_line));
		}

		// RULE 3:	LEFT-RIGHT
		results_.rule_3_y = Mathf.Min(Intersection_PointY(distance_.left.point_2, distance_.left.point_3, distance_line), Intersection_PointY(velocity_.right.point_1, velocity_.right.point_2, velocity_line));

		// RULE 4:	ZERO-LEFT
		if (distance_line >= 0) 
		{
			results_.rule_4_y = Mathf.Min (Intersection_PointY (distance_.zero.point_2, distance_.zero.point_3, distance_line), Intersection_PointY (velocity_.left.point_2, velocity_.left.point_3, velocity_line));
		} 
		else 
		{
			results_.rule_4_y = Mathf.Min (Intersection_PointY (distance_.zero.point_1, distance_.zero.point_2, distance_line), Intersection_PointY (velocity_.left.point_2, velocity_.left.point_3, velocity_line));
		}

		// RULE 5:	ZERO-ZERO
		if (distance_line >= 0) 
		{
			if (velocity_line >= 0)
			{
				results_.rule_5_y = Mathf.Min (Intersection_PointY (distance_.zero.point_2, distance_.zero.point_3, distance_line), Intersection_PointY (velocity_.zero.point_2, velocity_.zero.point_3, velocity_line));
			}
			else
			{
				results_.rule_5_y = Mathf.Min (Intersection_PointY (distance_.zero.point_2, distance_.zero.point_3, distance_line), Intersection_PointY (velocity_.zero.point_1, velocity_.zero.point_2, velocity_line));
			}
		} 
		else 
		{
			if (velocity_line >= 0)
			{
				results_.rule_5_y = Mathf.Min (Intersection_PointY (distance_.zero.point_1, distance_.zero.point_2, distance_line), Intersection_PointY (velocity_.zero.point_2, velocity_.zero.point_3, velocity_line));
			}
			else
			{
				results_.rule_5_y = Mathf.Min (Intersection_PointY (distance_.zero.point_1, distance_.zero.point_2, distance_line), Intersection_PointY (velocity_.zero.point_1, velocity_.zero.point_2, velocity_line));
			}
		}

		// RULE 6: ZERO-RIGHT
		if (distance_line >= 0) 
		{
			results_.rule_6_y = Mathf.Min (Intersection_PointY (distance_.zero.point_2, distance_.zero.point_3, distance_line), Intersection_PointY (velocity_.right.point_1, velocity_.right.point_2, velocity_line));
		} 
		else 
		{
			results_.rule_6_y = Mathf.Min (Intersection_PointY (distance_.zero.point_1, distance_.zero.point_2, distance_line), Intersection_PointY (velocity_.right.point_1, velocity_.right.point_2, velocity_line));
		}

		// RULE 7: RIGHT-LEFT
		results_.rule_7_y = Mathf.Min (Intersection_PointY (distance_.right.point_1, distance_.right.point_2, distance_line), Intersection_PointY (velocity_.left.point_2, velocity_.left.point_3, velocity_line));

		// RULE 8: RIGHT-ZERO
		if (velocity_line >= 0)
		{
			results_.rule_8_y = Mathf.Min (Intersection_PointY (distance_.right.point_1, distance_.right.point_2, distance_line), Intersection_PointY (velocity_.zero.point_2, velocity_.zero.point_3, velocity_line));
		}
		else
		{
			results_.rule_8_y = Mathf.Min (Intersection_PointY (distance_.right.point_1, distance_.right.point_2, distance_line), Intersection_PointY (velocity_.zero.point_1, velocity_.zero.point_2, velocity_line));
		}

		// RULE 9: RIGHT-RIGHT
		results_.rule_9_y = Mathf.Min (Intersection_PointY (distance_.right.point_1, distance_.right.point_2, distance_line), Intersection_PointY (velocity_.right.point_1, velocity_.right.point_2, velocity_line));
	}

	void Compare_Results ()
	{
		// V_RIGHT: RULE 1
		results_.v_right = results_.rule_1_y;

		// RIGHT: RULE 2-RULE 4
		results_.right = Mathf.Max(results_.rule_2_y, results_.rule_4_y);

		// ZERO: RULE 3-RULE 7-RULE 5
		results_.zero = Mathf.Max(results_.rule_3_y, results_.rule_5_y, results_.rule_7_y);

		// LEFT: RULE 6-RULE 8
		results_.left = Mathf.Max(results_.rule_6_y, results_.rule_8_y);

		// V_LEFT: RULE 9
		results_.v_left = results_.rule_9_y;
	}

	void Calculate_Output ()
	{
		// V_RIGHT
		// Points
		results_.vr.top_left = new Vector2 (Intersection_PointX(output_.v_right.point_1, output_.v_right.point_2, results_.v_right)
				, results_.v_right);
		results_.vr.top_right = new Vector2 (2.0f , results_.v_right);
		results_.vr.bottom_left = output_.v_right.point_1;
		results_.vr.bottom_right = output_.v_right.point_3;

		// Top Bottom and Height
		results_.vr.Trapezoid_TBH ();
		// Area and Center of mass
		results_.vr.Trapezoid_Area_Com ();

		// RIGHT
		// Points
		results_.r.top_left = new Vector2 (Intersection_PointX(output_.right.point_1, output_.right.point_2, results_.right) , results_.right);
		results_.r.top_right = new Vector2 (Intersection_PointX(output_.right.point_2, output_.right.point_3, results_.right) , results_.right);
		results_.r.bottom_left = output_.right.point_1;
		results_.r.bottom_right = output_.right.point_3;

		// Top Bottom and Height
		results_.r.Trapezoid_TBH ();
		// Area and Center of mass
		results_.r.Trapezoid_Area_Com ();

		// ZERO
		// Points
		results_.z.top_left = new Vector2 (Intersection_PointX(output_.zero.point_1, output_.zero.point_2, results_.zero) , results_.zero);
		results_.z.top_right = new Vector2 (Intersection_PointX(output_.zero.point_2, output_.zero.point_3, results_.zero) , results_.zero);
		results_.z.bottom_left = output_.zero.point_1;
		results_.z.bottom_right = output_.zero.point_3;

		// Top Bottom and Height
		results_.z.Trapezoid_TBH ();
		// Area and Center of mass
		results_.z.Trapezoid_Area_Com ();

		// LEFT
		// Points
		results_.l.top_left = new Vector2 (Intersection_PointX(output_.left.point_1, output_.left.point_2, results_.left) , results_.left);
		results_.l.top_right = new Vector2 (Intersection_PointX(output_.left.point_2, output_.left.point_3, results_.left) , results_.left);
		results_.l.bottom_left = output_.left.point_1;
		results_.l.bottom_right = output_.left.point_3;

		// Top Bottom and Height
		results_.l.Trapezoid_TBH ();
		// Area and Center of mass
		results_.l.Trapezoid_Area_Com ();

		// V_LEFT
		// Points
		results_.vl.top_left = new Vector2 (-2.0f, results_.v_left);
		results_.vl.top_right = new Vector2 (Intersection_PointX(output_.v_left.point_2, output_.v_left.point_3, results_.v_left) , results_.v_left);
		results_.vl.bottom_left = output_.v_left.point_1;
		results_.vl.bottom_right = output_.v_left.point_3;

		// Top Bottom and Height
		results_.vl.Trapezoid_TBH ();
		// Area and Center of mass
		results_.vl.Trapezoid_Area_Com ();

		// CALCULATE THE COMBINED CENTER OF MASS
		// (sum of the center of masses) / (sum of the areas)
		crisp_output = (results_.vr.com + results_.r.com + results_.z.com + results_.l.com + results_.vl.com) 
			/ (results_.vr.area + results_.r.area + results_.z.area + results_.l.area + results_.vl.area);
		print(crisp_output);
	}
}