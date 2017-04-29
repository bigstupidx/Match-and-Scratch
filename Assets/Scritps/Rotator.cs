using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;

public class Rotator : Circumference {

	public float speed = 100f;
	public float marginBetweenPins = 0.12f;

	private float spawnTimeDelay;
	private Circumference me;
	private List<Circumference> circumferencesCollided = new List<Circumference>();
	private Dictionary<int, PinsGroups> pinsGroups = new Dictionary<int, PinsGroups>();

	public override void Initialize() {
		me = this;
	}

	void FixedUpdate() {
		transform.Rotate(0f, 0f, speed * Time.deltaTime);
	}

	public void AddPin(Circumference newPin, Collider2D col) {
		newPin.colisionador.isTrigger = false;
		Pin cn = newPin.GetComponent<Pin>();
		cn.isPinned = true;
		newPin.transform.SetParent(transform);
		circumferencesCollided.Clear();
		SearchNearestPins(newPin);
		if(!GameManager.instance.gameHasEnded) {
			PlaySound (newPin.colorType);
			Reposition(newPin);

			// Volvemos a buscar por si al recolocar se generan nuevas colisiones
			//SearchNearestPins(newPin); // TODO: ESTO DA ERRORES....

			ProcessPin(newPin);
			spawnTimeDelay = ProcessPinsGroups();
			GameManager.instance.spawner.SpawnPin(spawnTimeDelay);
			if (GameManager.instance.canInverseDir) {
				speed *= -1;
			}
		}
	}

	void SearchNearestPins(Circumference newCircumference) {		
		// Comprobamos la distancia con el resto de bolas
		for (int i = 0; i < pinsGroups.Count; i++) {
			if (pinsGroups[i].isActive) {
				foreach (Circumference c in pinsGroups[i].members) {
					if ( IsColliding( newCircumference, c, marginBetweenPins ) ) {
						if (c.colorType !=  newCircumference.colorType) {
							GameManager.instance.EndGame();
							// Esto es solamente para que cuando salgamos del juego, este pin tb se borre.
							pinsGroups[pinsGroups.Count-1].AddMember(newCircumference);
							break;
						}
						else if (!circumferencesCollided.Contains(c))
							circumferencesCollided.Add (c);
					}
				}
			}
		}
		//Debug.LogFormat("Colisionando con {0} circumferencias: {1}", circumferencesCollided.Count, circumferencesCollided.ListaAsString());
	}

	void Reposition(Circumference newPin) {
		if ( IsColliding(newPin, me, marginBetweenPins) )
			circumferencesCollided.Add (me);

		// Si hay 3 o mas, nos quedamos sólo con los dos mas cercanos
		if (circumferencesCollided.Count > 2)  {
			circumferencesCollided.Sort( (A,B) => DistanceBetween(newPin.GetPosition(), A.GetPosition()).CompareTo(DistanceBetween(newPin.GetPosition(), B.GetPosition())) );
			circumferencesCollided = circumferencesCollided.GetRange(0, 2);
		}

		switch (circumferencesCollided.Count) {
			case 1:
				/*
				//debug posicion pin colisionado
				DrawTheGizmo (new GizmoToDraw( GizmoType.sphere, pinsCollided[0].GetPosition(), pinsCollided[0].GetRadius(), Color.gray ) );
				//debug posicion new pin en el momento de la collision
				DrawTheGizmo( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.yellow ) );
				*/
				// Reposición
				newPin.transform.position = circumferencesCollided[0].GetPosition() + ( (newPin.GetPosition() - circumferencesCollided[0].GetPosition() ).normalized * ( newPin.GetRadius() + circumferencesCollided[0].GetRadius() ) );
				//debug posicion new pin en despues de la colocación
				//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.green ) );
				if ( circumferencesCollided[0].tag == "Rotator" ) newPin.GetComponent<Pin>().DrawSpear();
			break;
			case 2:
				Circumference A = circumferencesCollided [0];
				Circumference B = circumferencesCollided [1];
				if (A == B)
					Debug.Log ("Error WTF 3: Hemos colisionador dos veces con el mismo Pin");
			
				//Solución de Fernando Rojas basada en: https://es.wikipedia.org/wiki/Teorema_del_coseno
				float Lc = (B.GetPosition () - A.GetPosition ()).magnitude; //A.GetRadius() + B.GetRadius();
				float La = B.GetRadius () + newPin.GetRadius ();
				float Lb = newPin.GetRadius () + A.GetRadius ();

				float a = Mathf.Rad2Deg * Mathf.Acos ((Lb * Lb + Lc * Lc - La * La) / (2 * Lb * Lc));

				Vector3 ab = (B.GetPosition () - A.GetPosition ()).normalized;

				Quaternion rot = Quaternion.AngleAxis (a, Vector3.forward);
				Vector3 Solution1 = A.GetPosition () + rot * ab * Lb;

				rot = Quaternion.AngleAxis (a, -Vector3.forward);
				Vector3 Solution2 = A.GetPosition () + rot * ab * Lb;

				#region "Otra solución - Solo funciona con circulos con igual radio"	
				/*/
				//Solución para el ajuste de posición. http://stackoverflow.com/questions/18558487/tangent-circles-for-two-other-circles
				// 1 Calculate distance from A to B -> |AB|:
				float AB = Vector3.Distance(A.GetPosition(), B.GetPosition());
				// 2 Checks whether a solution exist, it exist only if:
				Debug.Assert(AB <= 4 * newPin.GetComponent<Circumference>().GetRadius());
				// 3 If it exist, calculate half-point between points A and B:
				Vector2 H = new Vector2( A.GetPosition().x + ( (B.GetPosition().x - A.GetPosition().x) / 2 ), A.GetPosition().y + ( (B.GetPosition().y - A.GetPosition().y) / 2 ) );
				// 4 Create normalized perpendicular vector to line segment AB:
				Vector2 HC_perp_norm = new Vector2( (B.GetPosition().y - A.GetPosition().y) / AB, -(B.GetPosition().x - A.GetPosition().x) / AB );
				// 5 Calculate distance from this H point to C point -> |HC|:		                           
				float HCpos = Mathf.Abs( 0.5f * Mathf.Sqrt( 16 * ( newPin.GetRadius() * newPin.GetRadius() ) - (AB*AB) ) );
				float HCneg = -( 0.5f * Mathf.Sqrt( 16 * ( newPin.GetRadius() * newPin.GetRadius() ) - (AB*AB) ) );
				// Posibles soluciones
				Vector3 Solution1 = new Vector3 (H.x + (HCpos * HC_perp_norm.x), H.y + (HCpos * HC_perp_norm.y), 0);
				Vector3 Solution2 = new Vector3 (H.x + (HCneg * HC_perp_norm.x), H.y + (HCneg * HC_perp_norm.y), 0);
				*/
				#endregion
				// nos quedamos con la mas cercana al spawner
				Vector3 sol = DistanceBetween (Solution1, GameManager.instance.spawner.transform.position) <
			                  DistanceBetween (Solution2, GameManager.instance.spawner.transform.position) ? Solution1 : Solution2;
				
				if ( float.IsNaN(sol.x) ) {
					Debug.Log ("<color=red>Error WTF 2: Naaaaaaan</color>");
				}
				else
					// Posición final
					newPin.transform.position = sol;
				/*
				/// DEBUG ///
				//debug posicion new pin en el momento de la collision
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newCircumference.GetPosition(), newCircumference.GetRadius(), Color.white ) );
				//debug posicion pins colisionados
				//A
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, A.GetPosition(), A.GetRadius(), Color.green ) );
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, A.GetPosition(), A.GetPosition() + (B.GetPosition () - A.GetPosition()).normalized * A.GetRadius(), Color.green ) ); // Ra to Rb
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, A.GetPosition(), A.GetPosition() + (Solution1 - A.GetPosition()).normalized * A.GetRadius(), Color.green ) );// Linea A-Solution1
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, A.GetPosition(), A.GetPosition() + (Solution2 - A.GetPosition()).normalized * A.GetRadius(), Color.green ) );//Linea  B-Solution2
				//B
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, B.GetPosition(), B.GetRadius(), Color.green ) );
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, B.GetPosition(), B.GetPosition() + (A.GetPosition () - B.GetPosition()).normalized * B.GetRadius(), Color.green ) ); // Rb to Ra
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, B.GetPosition(), B.GetPosition() + (Solution1 - B.GetPosition()).normalized * B.GetRadius(), Color.green ) ); // Linea A-Solution1
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, B.GetPosition(), B.GetPosition() + (Solution2 - B.GetPosition()).normalized * B.GetRadius(), Color.green ) ); // Linea B-Solution2
				//Solution 1
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, Solution1, newCircumference.GetRadius(), Color.yellow ) );
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution1, Solution1 + (A.GetPosition () - Solution1).normalized * newCircumference.GetRadius(), Color.yellow ) ); // Linea Solution1-A
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution1, Solution1 + (B.GetPosition () - Solution1).normalized * newCircumference.GetRadius(), Color.yellow ) ); // Linea Solution1-B
				//Solution 2
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, Solution2, newCircumference.GetRadius(), Color.yellow ) );			
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution2, Solution2 + (A.GetPosition () - Solution2).normalized * newCircumference.GetRadius(), Color.yellow ) ); // Linea Solution2-A
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution2, Solution2 + (B.GetPosition () - Solution2).normalized * newCircumference.GetRadius(), Color.yellow ) ); // Linea Solution2-B
				//Posicion fianl decidida...
				//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.transform.position, newCircumference.GetRadius(), Color.black ) );
				*/
			break;
			default:				
				Debug.Log("<color=red> ERROR WTF 111: nomero de colisiones incorrectas: </color>" + circumferencesCollided.Count.ToString());
			break;
		}
	}

	public void ProcessPin(Circumference newCircumference) {
		// Comprobamos si sólo hay 
		if (circumferencesCollided.Count == 1) {
			if (circumferencesCollided [0].tag == "Rotator") { // Si la colisión es con el rotator
				pinsGroups.Add (pinsGroups.Count, new PinsGroups (pinsGroups.Count, newCircumference));
			} 
			else {
				for (int i = 0; i < pinsGroups.Count; i++) {
					if (pinsGroups [i].isActive) {
						if (pinsGroups [i].Contains (circumferencesCollided [0])) {
							pinsGroups [i].AddMember (newCircumference);
						}
					}
				}
			}
		}
		else if (circumferencesCollided.Count > 1){
			List<int> groupsCollided = new List<int>();
			for (int i = 0; i < circumferencesCollided.Count && !GameManager.instance.gameHasEnded; i++) {
				if (circumferencesCollided [i].tag == "Pin") {
					for (int j = 0; j < pinsGroups.Count; j++) {
						if (pinsGroups [j].isActive) {
							if (pinsGroups [j].Contains (circumferencesCollided [i])) {
								if (!groupsCollided.Contains (pinsGroups [j].index)) {
									groupsCollided.Add (pinsGroups [j].index);
								}
							}
						}
					}
				}
			}

			if ( groupsCollided.Count > 0 ) { // ... Si hemos localizado un grupo en el que ya existe la circumferencia que evaluamos, metemos la nueva en ese grupo.
				pinsGroups[groupsCollided[0]].AddMember (newCircumference);
				CombineGroups (groupsCollided);
			}
			else { // ... Si la circumferencia que evaluamos no pertenece a ningún grupo algo raro ha pasado y necesitamos un "salvoconducto"
				string log = "";
				foreach (var item in circumferencesCollided)
				{
					log += "\n - " + item.name;
				}
				Debug.Log ("<color=red>Error WTF(1): Los pins colisionados no están en ningún grupo. Esto no debería suceder</color> \n - Pin Evaluado: " + newCircumference.name + "\n - Pins Colisionados:" + log);
			}
		}
	}

	void CombineGroups(List<int> groupsCollided) {
		// Unificamos grupos si hemos colisionado con mas de uno
		if (groupsCollided.Count > 1) {
			int destiny = groupsCollided[0];
			for (int i = 1; i < groupsCollided.Count; i++) {
				int origin = groupsCollided[i];
				pinsGroups[destiny].AddMembers(pinsGroups[origin].members);
				pinsGroups[origin].CombineWith(destiny);
				//Debug.Log(string.Format("<color=yellow>Combinados Grupos {0} y {1} </color>", destiny, origin));
			}
		}
	}

	float ProcessPinsGroups() {	
		int totalPinsToDestroy = 0;	
		// Si encontramos un grupo de mas de dos miembros del mismo color...
		for(int i = 0; i < pinsGroups.Count; i++) {
			if (pinsGroups[i].isActive){
				if (pinsGroups[i].Count > 2) {
					totalPinsToDestroy += pinsGroups[i].Count;
					// ...eliminamos el grupo.
					pinsGroups[i].Erase();
				}
			}
		}
		return Spawner.MINIMUM_SPAWN_TIME + (totalPinsToDestroy * Pin.TIME_TO_DESTROY);
	}

	public void EraseAllPins() {
		for (int i = 0; i < pinsGroups.Count; i++) {
			StartCoroutine(pinsGroups[i].DestroyMembers(false));
		}
	}

	float DistanceBetween(Vector3 A, Vector3 B) {
		return Mathf.Round( (B-A).sqrMagnitude * 100 ) / 100;
	}

	bool IsColliding(Circumference A, Circumference B, float margin = 0f) {
		if (A.colorType == B.colorType)
			return DistanceBetween( A.GetPosition(),B.GetPosition() ) < ( (A.GetRadius() + B.GetRadius() + margin) * (A.GetRadius() + B.GetRadius() + margin) );
		else
			return DistanceBetween( A.GetPosition(),B.GetPosition() ) < ( (A.GetRadius() + B.GetRadius()) * (A.GetRadius() + B.GetRadius()) );
	}

	public void Reset() {
		circumferencesCollided.Clear();
		GameObject[] pins = GameObject.FindGameObjectsWithTag("Pin");
		Debug.Log(string.Format("Encontrados {0} Pins", pins.Length));
		for (int i = 0; i < pins.Length; i++) {
			Destroy(pins[i]);
		}
		pinsGroups.Clear();
		speed = 100f;
	}

	void PlaySound(int id) {
		switch (id) {
			case 0:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_1);
			break;
			case 1:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_2);
			break;
			case 2:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_3);
			break;
			case 4:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_4);
			break;
			case 5:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_5);
			break;
			case 6:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_6);
			break;
			case 7:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_7);
			break;
			case 8:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_8);
			break;
			case 9:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_9);
			break;
			case 10:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_10);
			break;
			default:
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_1);
			break;
		}
	}

	/*   
	/// DEBUG 
	void PrintColorGroupsLog(string enunciado = "") {
		string log = enunciado.Length <= 0 ? "" : enunciado + ": \n";
		for(int i = 0; i < colorGroups.Count; i++) {
			for( int j = 0; j < colorGroups[i].Count; j++) {
				log += (colorGroups[i][j].name + " ");
			}
			log += "\r\n";
		}
		Debug.Log (log);
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
	*/
}
