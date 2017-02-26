using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : Circumference {

	public float speed = 100f;

	private List<GameObject> pins = new List<GameObject>();
	private List<GameObject> pinsCollided = new List<GameObject>();                     

	private List<GizmoToDraw> gizmosToDraw = new List<GizmoToDraw>();

	public override void Initialize() {
		pins.Clear();		
	}

	void Update() {
		transform.Rotate(0f, 0f, speed * Time.deltaTime);
	}

	public void AddPin(GameObject newPin) {
		ColorNeedle cn =newPin.GetComponent<ColorNeedle>();

		Reposition(newPin);
		pins.Add(newPin);
		cn.GetComponent<CircleCollider2D>().isTrigger = false;
		cn.isPinned = true;
		newPin.transform.SetParent(transform);

		/// ******** ///
		EvaluateColorNeedles(newPin);
		/// ******** ///

		GameManager.instance.spawner.SpawnNeedle();
	}

	void Reposition(GameObject newPin) {
		//float newPinRad = newPin.GetComponent<ColorNeedle>().radius;
		Circumference newCircumference = newPin.GetComponent<Circumference>();
		pinsCollided.Clear();
		// Comprobamos ladistanciacon el resto debolas
		foreach (GameObject pin in pins) {
			float collidedPinRad = pin.GetComponent<Circumference>().GetRadius();
			if ( GetDistanceBetween(pin.transform.position, newCircumference.GetPosition()) < ((newCircumference.GetRadius() + collidedPinRad) * (newCircumference.GetRadius() + collidedPinRad))) {
				pinsCollided.Add (pin);
			}
		}
		// Y tambien la distanciacon el rotor
		if ( GetDistanceBetween(transform.position, newPin.transform.position) < ((newCircumference.GetRadius() + GetRadius()) * (newCircumference.GetRadius() + GetRadius()))) {
			pinsCollided.Add (gameObject);
		}
		// Si hay 3 o mas, nos quedamos sólo con los dos mas cercanos
		if (pinsCollided.Count > 2)  {
			//pinsCollided = GetOnlyNearest(newPin, 2);
			pinsCollided.Sort((X,Y) => X.GetComponent<Circumference>().GetRadius().CompareTo(Y.GetComponent<Circumference>().GetRadius()));
			pinsCollided = pinsCollided.GetRange(0,2);
		}

		switch (pinsCollided.Count) {
		case 1:
			Circumference pinnedCirc = pinsCollided[0].GetComponent<Circumference>();
			//debug posicion pin colisionado
			//DrawTheGizmo (new GizmoToDraw( GizmoType.sphere, pinnedCirc.GetPosition(), pinnedCirc.GetRadius(), Color.gray ) );
			//debug posicion new pin en el momento de la collision
			//DrawTheGizmo( new GizmoToDraw( GizmoType.sphere, newCircumference.GetPosition(), newCircumference.GetRadius(), Color.yellow ) );
			//  -- recolocacion --
			newPin.transform.position = pinnedCirc.GetPosition() + ( (newCircumference.GetPosition() - pinnedCirc.GetPosition()).normalized *  (newCircumference.GetRadius() + pinnedCirc.GetRadius()) );
			//debug posicion new pin en despues de la colocación
			//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newCircumference.GetPosition(), newCircumference.GetRadius(), Color.green ) );
			if ( pinsCollided[0].tag == "Rotator" ) {
				newPin.GetComponent<ColorNeedle>().DrawSpear();
			}
			break;
		case 2:
			Circumference A = pinsCollided[0].GetComponent<Circumference>();
			Circumference B = pinsCollided[1].GetComponent<Circumference>();
			//Solución para el ajuste de posición. http://stackoverflow.com/questions/18558487/tangent-circles-for-two-other-circles
			// 1 Calculate distance from A to B -> |AB|:
			float AB = Vector3.Distance(A.GetPosition(), B.GetPosition());
			// 2 Checks whether a solution exist, it exist only if:
			Debug.Assert(AB <= 4 * newPin.GetComponent<Circumference>().GetRadius());
			// 3 If it exist, calculate half-point between points A and B:
			Vector2 H = new Vector2( A.GetPosition().x + ( (B.GetPosition().x - A.GetPosition().x) / 2 ), 
			                         A.GetPosition().y + ( (B.GetPosition().y - A.GetPosition().y) / 2 ) 
			                       );
			// 4 Create normalized perpendicular vector to line segment AB:
			Vector2 HC_perp_norm = new Vector2( (B.GetPosition().y - A.GetPosition().y) / AB, 
			                                   -(B.GetPosition().x - A.GetPosition().x) / AB 
			                                  );
			// 5 Calculate distance from this H point to C point -> |HC|:		                           
			float HCpos = Mathf.Abs( 0.5f * Mathf.Sqrt( 16 * ( newCircumference.GetRadius() * newCircumference.GetRadius() ) - (AB*AB) ) );
			float HCneg = -( 0.5f * Mathf.Sqrt( 16 * ( newCircumference.GetRadius() * newCircumference.GetRadius() ) - (AB*AB) ) );
			// Posibles soluciones
			Vector3 Solution1 = new Vector3 (H.x + (HCpos * HC_perp_norm.x), H.y + (HCpos * HC_perp_norm.y), 0);
			Vector3 Solution2 = new Vector3 (H.x + (HCneg * HC_perp_norm.x), H.y + (HCneg * HC_perp_norm.y), 0);
			// nos quedamos con la mas cercana al spawner
			Vector3 sol = GetDistanceBetween(Solution1, GameManager.instance.spawner.transform.position) < 
						  GetDistanceBetween(Solution2, GameManager.instance.spawner.transform.position) ? Solution1 : Solution2;

			// Posición final
			newPin.transform.position = sol;

			/// DEBUG ///
			//debug posicion new pin en el momento de la collision
			//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newCircumference.GetPosition(), newCircumference.GetRadius(), Color.white ) );
			//debug posicion pins colisionados
			//A
			//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, A.GetPosition(), A.GetRadius(), Color.red ) );
			//B
			//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, B.GetPosition(), B.GetRadius(), Color.green ) );
			//C
			//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, Solution1, newCircumference.GetRadius(), Color.cyan ) );
			//D
			//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, Solution2, newCircumference.GetRadius(), Color.blue ) );;
			//Posicion fianl decidida...
			//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.transform.position, newCircumference.GetRadius(), Color.black ) );
			break;
		default:				
			Debug.Log("<color=red> ERROR WTF 111 </color>");
			break;
		}
	}

	float GetDistanceBetween(Vector3 A, Vector3 B) {
		return (B-A).sqrMagnitude;
	}

	void DrawTheGizmo(GizmoToDraw g) {
		if (!gizmosToDraw.Contains(g))
			gizmosToDraw.Add(g);
	}
	
	void OnDrawGizmos() {
		foreach (GizmoToDraw gtd in gizmosToDraw) {
			switch (gtd.gizmoType) {
			case GizmoType.line:
				Gizmos.color = gtd.color;
				Gizmos.DrawLine(gtd.from, gtd.to);
				break;
			case GizmoType.sphere:
				Gizmos.color = gtd.color;
				Gizmos.DrawWireSphere(gtd.from, gtd.size);
				break;
			}
		}
	}


}
