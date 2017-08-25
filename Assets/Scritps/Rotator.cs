using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using System;

public class Rotator : Circumference {
	public const float INITIAL_SPEED = 100f;
	[SerializeField]
	private float rotationSpeed;
	public float RotationSpeed {
		get { return rotationSpeed;}
		set { rotationSpeed = value;}
	}

	public int rotationDirection = 1;

	public float marginBetweenPins = 0.08f;
	public float currentSpeed;


	[SerializeField]
	private float variableSpeedInc;

	public bool canInverseDir;
	public bool canUseCrazySpeed;
	public float smoothCurrentSpeed;

	private float[] speedIncs = new float[]{ -2.0f, -1.5f, 0, 0.5f};
	private float newCrazySpeedInc;

	private float spawnTimeDelay;

	private Circumference me;
	private List<Circumference> circumferencesCollided = new List<Circumference>();
	private Dictionary<int, ColorPinsGroup> groupsDictionary = new Dictionary<int, ColorPinsGroup>();
	private List<int> groupsCollided = new List<int>();

	float angleRotated = 0;

	public Action OnPinPinned;
	public Action OnCompleteRotation;

	public override void Initialize() {
		me = this;
	}

	void FixedUpdate() {		
		if (!GameManager.instance.isGamePaused) {
			currentSpeed = (RotationSpeed + (RotationSpeed * variableSpeedInc)) * rotationDirection;
			smoothCurrentSpeed = currentSpeed * Time.fixedDeltaTime;
			transform.Rotate (0f, 0f, smoothCurrentSpeed);
		
			if (OnCompleteRotation != null) {
				angleRotated += smoothCurrentSpeed;
				if (angleRotated >= 360) {
					angleRotated = 0;
					OnCompleteRotation ();
				}
			}
		}
	}

	public void AddPin(Pin newPin, GameObject col) {
		if (OnPinPinned != null)
			OnPinPinned ();
		
		newPin.colisionador.isTrigger = false;
		newPin.SetPinned ();
		newPin.colisionador.enabled = true;


		/*
		if (col.name == "Rotator")
			Debug.Log (string.Format ("{0} collisiona con {1}", newPin.name, col.name));
		else {
			Debug.Log (string.Format ("{0} collisiona con {1} que pertenece al grupo {2} y su estado es {3} y contiene {4} miembros.", newPin.name, col.name, newPin.colorGroup.ToString (), groupsDictionary [newPin.colorGroup].currentState.ToString (), groupsDictionary [newPin.colorGroup].Count.ToString ()));
		}
		*/

		AddToParent (newPin); 		// Metemos el Pin en el rotator
		SearchNearestPins (newPin);	// Bucamos cercanos
		Reposition (newPin); 		// Recolocamos
		SearchNearestPins (newPin, false);	// Volvemos a buscar por si al recolocar se generan nuevas colisiones

		if (IsGameOver(newPin)) {
			GameManager.instance.GameOver ();
			groupsDictionary[groupsDictionary.Count-1].AddMember (newPin); // Metemos el pin en el ultimo grupo para que se elimine al terminar 
		} else {
			ProcessPin (newPin);
			spawnTimeDelay = ProcessPinsGroups ();
			GameManager.instance.spawner.SpawnPin (spawnTimeDelay);
			if (canInverseDir) {
				rotationDirection *= -1;
			}
		}
	}

	void AddToParent(Circumference newPin) {
		newPin.transform.SetParent(transform);
		PlaySound (newPin.colorType);
	}

	void SearchNearestPins(Circumference newPin, bool startOver = true) {
		if (startOver)
			circumferencesCollided.Clear();
		
		// Comprobamos la distancia con el resto de bolas
		for (int i = 0; i < groupsDictionary.Count; i++) {
			if (groupsDictionary[i].isActive) {
				for (int c = 0; c < groupsDictionary[i].members.Count; c++) {
					if ( IsColliding( newPin,  groupsDictionary[i].members[c], marginBetweenPins ) ) {
						if (!circumferencesCollided.Contains(groupsDictionary[i].members[c]))
							circumferencesCollided.Add (groupsDictionary[i].members[c]);
					}	
				}
			}
		}

		// Comprobamos la distancia con el rotator
		if (IsColliding (newPin, me)) {
			if (!circumferencesCollided.Contains (me)) {
				circumferencesCollided.Add (me);
			}
		}
		
		if (circumferencesCollided.Count == 0) {
			//Debug.Log ("<color=red>Error WTF (0001): No se ha encontrado ninguna colision</color>");
			AnalyticsSender.SendCustomAnalitycs ("wtfError", new Dictionary<string, object>() {
				{"type", "0001"},
				{"message", "No se ha encontrado ninguna colision"}
			});
		}
	}

	bool IsGameOver(Circumference newPin) {
		bool collidedWithDifferent = circumferencesCollided.Exists(c => c.colorType != newPin.colorType && c.tag != "Rotator");
		return collidedWithDifferent;			
	}

	void Reposition(Circumference newPin) {		

		//foreach (Circumference c in circumferencesCollided)
		//	Debug.LogFormat ("<color=blue>Colision con {0} ({1}): distancia: {2}</color>", c.name, c.tag, DistanceBetweenCircumferences (newPin, c));

		if (circumferencesCollided.Count > 1)
			circumferencesCollided.Sort( (A,B) => DistanceBetweenCircumferences(newPin, A).CompareTo(DistanceBetweenCircumferences(newPin, B)) );

		// Si mas de dos, nos quedamos sólo con los dos mas cercanos
		if (circumferencesCollided.Count > 2)  {
			circumferencesCollided = circumferencesCollided.GetRange(0, 2);
		}

		//foreach (Circumference c in circumferencesCollided)
		//	Debug.LogFormat ("<color=yellow>Colision aceptada: {0} ({1}) ( distancia: {2})</color>", c.name, c.tag, DistanceBetweenCircumferences (newPin, c));
		switch (circumferencesCollided.Count) {
			case 1:
				/*
				//debug posicion pin colisionado
				DrawTheGizmo (new GizmoToDraw( GizmoType.sphere, pinsCollided[0].GetPosition(), pinsCollided[0].GetRadius(), Color.gray ) );
				//debug posicion new pin en el momento de la collision
				DrawTheGizmo( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.yellow ) );
				*/
				
				// Reposición
				//newPin.transform.position = circumferencesCollided[0].GetPosition() + ( ( newPin.GetPosition() - circumferencesCollided[0].GetPosition() ).normalized * ( newPin.GetRadius() + circumferencesCollided[0].GetRadius() ) );
				newPin.transform.position = circumferencesCollided[0].GetPosition() + SnapNormalizedVector(newPin.GetPosition(), circumferencesCollided[0].GetPosition()) * ( newPin.GetRadius() + circumferencesCollided[0].GetRadius() );
				
			//debug posicion new pin despues de la colocación
				//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.green ) );
				if ( circumferencesCollided[0].tag == "Rotator" ) newPin.GetComponent<Pin>().DrawSpear();
			break;
		case 2:
				Circumference A = circumferencesCollided [0];
				Circumference B = circumferencesCollided [1];
				if (A == B) {
					//Debug.Log ("Error WTF 0002: Hemos colisionador dos veces con el mismo Pin");
				AnalyticsSender.SendCustomAnalitycs ("wtfError", new Dictionary<string, object>() {
						{"type", "0002"},
						{"message", "Hemos colisionador dos veces con el mismo Pin"}
					});
				}
			
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
				/*
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
				Vector3 sol = DistanceBetweenPoints(Solution1, GameManager.instance.spawner.transform.position) <
			                  DistanceBetweenPoints(Solution2, GameManager.instance.spawner.transform.position) ? Solution1 : Solution2;

				if ( float.IsNaN( sol.x) ) {
					//Debug.Log ("<color=red>Error WTF 0003: Naaaaaaan</color>");
					AnalyticsSender.SendCustomAnalitycs ("wtfError", new Dictionary<string, object>() {
						{"type", "0003"},
						{"message", "La reposición contiene numero inválidos: (" + sol.ToString() + "). Cambio NaN -> cero"}
					});
					sol = new Vector3 (float.IsNaN (sol.x) ? 0 : sol.x, float.IsNaN (sol.y) ? 0 : sol.y, float.IsNaN (sol.y) ? 0 : sol.y);
					// TODO: Enviar como estadística de errores
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
				//Debug.Log(string.Format("<color=red> ERROR WTF 0004: número de colisiones incorrectas: {0}</color>", circumferencesCollided.Count.ToString()));

				AnalyticsSender.SendCustomAnalitycs ("wtfError", new Dictionary<string, object>() {
					{"type", "0004"},
					{"message", "número de colisiones incorrectas: (" + circumferencesCollided.Count.ToString() + ")"}
				});
			break;
		}
	}

	float circleDivisions = 360f / 8f;
	Vector3 SnapNormalizedVector(Vector3 newPinPos, Vector3 collidedPinPos) {
		//float angleBetween = Vector3.Angle (B, A); // Angulo de nuestra bola respecto de la que ha colisionado

		Vector3 snappedDirection = SnapToAngle (newPinPos, collidedPinPos);
		return -snappedDirection;
	}

	/// <summary>
	/// Fija un angulo dado a las divisiones de una circunferencia.
	/// </summary>
	/// <returns>The to.</returns>
	/// <param name="v3">V3.</param>
	/// <param name="snapAngle">Snap angle.</param>
	/// <param name="url">http://answers.unity3d.com/questions/493006/snap-a-direction-vector.html</param> 
	Vector3 SnapToAngle(Vector3 newPinPos, Vector3 collidedPinPos) {
		
		Vector3 axisDirection;

		if (me.GetPosition () == collidedPinPos)
			axisDirection = Vector3.down;
		else
			axisDirection = (collidedPinPos - Vector3.zero).normalized;

		float deltaAngle = Vector3.Angle(Vector3.down, axisDirection);

		Vector3 dir = (collidedPinPos - newPinPos).normalized;
		float angle = Vector3.Angle(axisDirection, dir);
			
		//float badAngle = angle;

		//angle = angle.RoundToNearest(circleDivisions); // Deja esto calculado de antes si puedes

		//angulo entre rotor y bola puesta respecto de V3.down
		//angulo entre bola lanzada y bola puesta respecto del angulo anterior
		// Calculamos el angulo al que se debería pegar la bola lanzada a la bola puesta
		// devolvemos la posición correcta de la bola lanzada

		/*
		float angle = Vector3.Angle(Vector3.down, dir);
		float badAngle = angle;

		angle = angle.RoundToNearest(circleDivisions); // Deja esto calculado de antes si puedes
		*/
		float finalAng = (angle + deltaAngle).RoundToNearest (circleDivisions);

		//Debug.LogFormat ("Angulo {0} -> snappedTo {1}  ==> angulo final ({1} + {2}) = {3} -> snappedTo {4}", badAngle.ToString(), angle.ToString (),  deltaAngle.ToString(), (angle + deltaAngle).ToString(), finalAng.ToString());

		return new Vector3(Mathf.Sin(finalAng * Mathf.Deg2Rad) * Mathf.Sign(dir.x), -Mathf.Cos(finalAng * Mathf.Deg2Rad)* Mathf.Sign(dir.y), 0);
	}

	public void ProcessPin(Circumference newCircumference) {
		// Comprobamos si sólo hay 
		if (circumferencesCollided.Count == 1) {
			if (circumferencesCollided [0].tag == "Rotator") { // Si la colisión es con el rotator
				groupsDictionary.Add (groupsDictionary.Count, new ColorPinsGroup (groupsDictionary.Count, newCircumference));
			} 
			else {
				for (int i = 0; i < groupsDictionary.Count; i++) {
					if (groupsDictionary [i].isActive) {
						if (groupsDictionary [i].Contains (circumferencesCollided [0])) {
							groupsDictionary [i].AddMember (newCircumference);
						}
					}
				}
			}
		}
		else if (circumferencesCollided.Count > 1){
			groupsCollided.Clear ();
			for (int i = 0; i < circumferencesCollided.Count && !GameManager.instance.isGameOver; i++) {
				if (circumferencesCollided [i].tag == "Pin") {
					for (int j = 0; j < groupsDictionary.Count; j++) {
						if (groupsDictionary [j].isActive) {
							if (groupsDictionary [j].Contains (circumferencesCollided [i])) {
								if (!groupsCollided.Contains (groupsDictionary [j].index)) {
									groupsCollided.Add (groupsDictionary [j].index);
								}
							}
						}
					}
				}
			}

			if ( groupsCollided.Count > 0 ) { // ... Si hemos localizado un grupo en el que ya existe la circumferencia que evaluamos, metemos la nueva en ese grupo.
				groupsDictionary[groupsCollided[0]].AddMember (newCircumference);
				CombineGroups (groupsCollided);
			}
			else { // ... Si la circumferencia que evaluamos no pertenece a ningún grupo algo raro ha pasado y necesitamos un "salvoconducto"
				string log = "";
				foreach (var item in circumferencesCollided)
				{
					log += "\n - " + item.name;
				}
				//Debug.Log ("<color=red>Error WTF 0005: Los pins colisionados no están en ningún grupo. Esto no debería suceder</color> \n - Pin Evaluado: " + newCircumference.name + "\n - Pins Colisionados:" + log);
				AnalyticsSender.SendCustomAnalitycs ("wtfError", new Dictionary<string, object>() {
					{"type", "0005"},
					{"message", string.Format("Los pins colisionados no están en ningún grupo. \nPin Evaluado: {0} - \nPin Colisionados: {1}", newCircumference.name, log)}
				});
			}
		}
	}

	void CombineGroups(List<int> groupsCollided) {
		// Unificamos grupos si hemos colisionado con mas de uno
		if (groupsCollided.Count > 1) {
			int destiny = groupsCollided[0];
			for (int i = 1; i < groupsCollided.Count; i++) {
				int origin = groupsCollided[i];
				groupsDictionary[destiny].AddMembers(groupsDictionary[origin].members);
				groupsDictionary[origin].CombineWith(destiny);
				//Debug.Log(string.Format("<color=yellow>Combinados Grupo {0} en {1} </color>", destiny, origin));
			}
		}
	}

	float ProcessPinsGroups() {	
		int totalPinsToDestroy = 0;	
		// Si encontramos un grupo de mas de dos miembros del mismo color...
		for(int i = 0; i < groupsDictionary.Count; i++) {
			if (groupsDictionary[i].isActive) {
				if (groupsDictionary[i].Count > 2) {
					totalPinsToDestroy += groupsDictionary[i].Count;
					// ...eliminamos el grupo.
					groupsDictionary[i].Erase();
				}
			}
		}
		return Spawner.MINIMUM_SPAWN_TIME;// + (totalPinsToDestroy * Pin.TIME_TO_DESTROY);
	}

	public void EraseAllPins() {
		foreach( KeyValuePair<int, ColorPinsGroup> pg in groupsDictionary)
			StartCoroutine(pg.Value.DestroyMembers(false));
	}

	float DistanceBetweenPoints(Vector3 A, Vector3 B) {
		float d = Mathf.Round( (B-A).sqrMagnitude * 100 ) / 100;
		return d;
	}

	float DistanceBetweenCircumferences(Circumference A, Circumference B) {
		float d = DistanceBetweenPoints(A.GetPosition (), B.GetPosition ()) - ((A.GetRadius () + B.GetRadius ()) * (A.GetRadius () + B.GetRadius ()));
		return d;
	}

	bool IsColliding(Circumference A, Circumference B, float margin = 0f) {
		bool ret;
		if (A.colorType == B.colorType || B.colorType == -1) {
			ret = DistanceBetweenCircumferences(A, B) <= margin;
		}
		else {
			ret = DistanceBetweenCircumferences(A, B) <= 0;
		}
		return ret;
	}

	public void Reset() {
		circumferencesCollided.Clear();
		GameObject[] pins = GameObject.FindGameObjectsWithTag("Pin");
		//Debug.Log(string.Format("Encontrados {0} Pins", pins.Length));
		for (int i = 0; i < pins.Length; i++) {
			Destroy(pins[i]);
		}
		groupsDictionary.Clear();
		StopInverseDirection ();
		StopCrazySpeed ();
		RotationSpeed = INITIAL_SPEED;
		variableSpeedInc = 0;
		rotationDirection = 1;
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

	public void StartInverseDirection() {
		canUseCrazySpeed = false;
		canInverseDir = true;
	}

	public void StopInverseDirection() {
		canInverseDir = false;
	}

	public void StartCrazySpeed() {
		canInverseDir = false;
		canUseCrazySpeed = true;
		StartCoroutine(CrazySpeedDifficult());
	}

	public void StopCrazySpeed() {
		StopCoroutine(CrazySpeedDifficult());
		canUseCrazySpeed = false;
		variableSpeedInc = 0;
	}

	public IEnumerator CrazySpeedDifficult() {
		while (canUseCrazySpeed && !GameManager.instance.isGameOver) {
			float tmpInc = speedIncs[UnityEngine.Random.Range (0, speedIncs.Length)];
			// No permitimos que salga dos veces el mismo numero
			while (newCrazySpeedInc == tmpInc) {
				tmpInc = speedIncs [UnityEngine.Random.Range (0, speedIncs.Length)];
			}
			newCrazySpeedInc = tmpInc;
			//Debug.Log ("newCrazySpeedInc: " + newCrazySpeedInc);
			StartCoroutine(SmoothSpeedIncrement(variableSpeedInc, newCrazySpeedInc, 1f));

			yield return new WaitForSeconds ( (float)UnityEngine.Random.Range (4,2));
		}
	}

	public IEnumerator SmoothSpeedIncrement(float from, float to, float time) {
		float elapsedTime = 0;
		while (elapsedTime < time) {
			elapsedTime += Time.deltaTime;
			variableSpeedInc = Mathf.Lerp(from, to, elapsedTime / time);
			yield return null;
		}
		//Debug.LogFormat ("<color=blue>From: {0} \n to: {1} \n variableSpeedInc: {2}</color>", from, to, variableSpeedInc);
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
