using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using System;

public class Rotator : Circumference
{
    public const float INITIAL_SPEED = 100f;
    public const float PINS_GAP = 0.08f;
    public const int PIN_MEMBERS_PER_GROUP = 3;

    public int rotationDirection = 1;
    public float currentSpeed;
    public float smoothCurrentSpeed;
    public bool canInverseDir;
    public bool canUseCrazySpeed;

    [SerializeField]
    private float rotationSpeed;
    [SerializeField]
    private float variableSpeedInc;
    private float angleRotated = 0;
    private float spawnTimeDelay;
    private float newCrazySpeedInc;
    private float[] speedIncs = new float[]{ -2.0f, -1.5f, 0, 0.5f };
    private Circumference me;
    private List<Circumference> circumferencesCollided = new List<Circumference>();
    private List<int> groupsCollided = new List<int>();
    private Dictionary<int, ColorPinsGroup> groupsDictionary = new Dictionary<int, ColorPinsGroup>();

    public Action OnPinPinned;
    public Action OnCompleteRotation;

    public float RotationSpeed
    {
        get { return rotationSpeed; }
        set { rotationSpeed = value; }
    }

    public override void Initialize()
    {
        me = this;
    }

    void FixedUpdate()
    {		
        if (!GameManager.Instance.isGamePaused)
        {
            currentSpeed = (RotationSpeed + (RotationSpeed * variableSpeedInc)) * rotationDirection;
            smoothCurrentSpeed = currentSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, 0f, smoothCurrentSpeed);
		
            if (OnCompleteRotation != null)
            {
                angleRotated += smoothCurrentSpeed;
                if (angleRotated >= 360)
                {
                    angleRotated = 0;
                    OnCompleteRotation();
                }
            }
        }
    }

    public void AddPin(Pin newPin, GameObject col)
    {
        if (OnPinPinned != null)
            OnPinPinned();
		
        newPin.colisionador.isTrigger = false;
        newPin.SetPinned();
        newPin.colisionador.enabled = true;

        // Change pin parent
        AddToParent(newPin);
        // Find near pins
        SearchNearestPins(newPin);
        // fix the position
        Reposition(newPin);
        // Nees to find nearest pins again before reposition
        SearchNearestPins(newPin, false);

        if (IsGameOver(newPin))
        {
            GameManager.Instance.GameOver();
            // Add las pin to the last created group of pins to remove when finish game
            groupsDictionary[groupsDictionary.Count - 1].AddMember(newPin);
        }
        else
        {
            ProcessPin(newPin);
            spawnTimeDelay = ProcessPinsGroups();
            GameManager.Instance.spawner.SpawnPin(spawnTimeDelay);
            if (canInverseDir)
            {
                rotationDirection *= -1;
            }
        }
    }

    void AddToParent(Circumference newPin)
    {
        newPin.transform.SetParent(transform);
        PlaySound(newPin.colorType);
    }

    void SearchNearestPins(Circumference newPin, bool startOver = true)
    {
        if (startOver)
            circumferencesCollided.Clear();
		
        // First find collisions with nearest pins
        for (int i = 0; i < groupsDictionary.Count; i++)
        {
            if (groupsDictionary[i].isActive)
            {
                for (int c = 0; c < groupsDictionary[i].members.Count; c++)
                {
                    if (IsColliding(newPin, groupsDictionary[i].members[c], PINS_GAP))
                    {
                        if (!circumferencesCollided.Contains(groupsDictionary[i].members[c]))
                            circumferencesCollided.Add(groupsDictionary[i].members[c]);
                    }	
                }
            }
        }

        // And then check if collides with me (rotator)
        if (IsColliding(newPin, me))
        {
            if (!circumferencesCollided.Contains(me))
            {
                circumferencesCollided.Add(me);
            }
        }
		
        if (circumferencesCollided.Count == 0)
        {
            AnalyticsSender.SendCustomAnalitycs("wtfError", new Dictionary<string, object>()
                {
                    { "type", "0001" },
                    { "message", "No se ha encontrado ninguna colision" }
                }
            );
        }
    }

    bool IsGameOver(Circumference newPin)
    {
        bool collidedWithDifferent = circumferencesCollided.Exists(c => c.colorType != newPin.colorType && c.tag != "Rotator");
        return collidedWithDifferent;			
    }

    void Reposition(Circumference newPin)
    {
        if (circumferencesCollided.Count > 1)
        {
            circumferencesCollided.Sort((A, B) => DistanceBetweenCircumferences(newPin, A).CompareTo(DistanceBetweenCircumferences(newPin, B)));
        }

        // If collithes with more than 2 at same time, only take nearest 2 to reposition
        if (circumferencesCollided.Count > 2)
        {
            circumferencesCollided = circumferencesCollided.GetRange(0, 2);
        }

        switch (circumferencesCollided.Count)
        {
            case 1:
                #if VISUAL_DEBUG
    				//debug posicion pin colisionado
                    DrawTheGizmo (new GizmoToDraw( GizmoType.sphere, circumferencesCollided[0].GetPosition(), circumferencesCollided[0].GetRadius(), Color.gray ) );
    				//debug posicion new pin en el momento de la collision
    				DrawTheGizmo( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.yellow ) );
				#endif
				
				// Correct the position
                newPin.transform.position = circumferencesCollided[0].GetPosition() + SnapNormalizedVector(newPin.GetPosition(), circumferencesCollided[0].GetPosition()) * (newPin.GetRadius() + circumferencesCollided[0].GetRadius());
				
                #if VISUAL_DEBUG
    			    //debug posicion new pin despues de la colocación
    				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.green ) );
                #endif

                // If collides oly with rotator then draw the spear
                if (circumferencesCollided[0].tag == "Rotator")
                    newPin.GetComponent<Pin>().DrawSpear();
                break;
            case 2:
                Circumference A = circumferencesCollided[0];
                Circumference B = circumferencesCollided[1];

                if (A == B)
                {
                    AnalyticsSender.SendCustomAnalitycs("wtfError", new Dictionary<string, object>()
                        {
                            { "type", "0002" },
                            { "message", "Hemos colisionador dos veces con el mismo Pin" }
                        }
                    );
                }
			
				//Solución de Fernando Rojas basada en: https://es.wikipedia.org/wiki/Teorema_del_coseno
                float Lc = (B.GetPosition() - A.GetPosition()).magnitude; //A.GetRadius() + B.GetRadius();
                float La = B.GetRadius() + newPin.GetRadius();
                float Lb = newPin.GetRadius() + A.GetRadius();

                float a = Mathf.Rad2Deg * Mathf.Acos((Lb * Lb + Lc * Lc - La * La) / (2 * Lb * Lc));

                Vector3 ab = (B.GetPosition() - A.GetPosition()).normalized;

                Quaternion rot = Quaternion.AngleAxis(a, Vector3.forward);
                Vector3 Solution1 = A.GetPosition() + rot * ab * Lb;

                rot = Quaternion.AngleAxis(a, -Vector3.forward);
                Vector3 Solution2 = A.GetPosition() + rot * ab * Lb;

				// nos quedamos con la mas cercana al spawner
                Vector3 sol = DistanceBetweenPoints(Solution1, GameManager.Instance.spawner.transform.position) <
                              DistanceBetweenPoints(Solution2, GameManager.Instance.spawner.transform.position) ? Solution1 : Solution2;

                if (float.IsNaN(sol.x))
                {
                    AnalyticsSender.SendCustomAnalitycs("wtfError", new Dictionary<string, object>()
                        {
                            { "type", "0003" },
                            { "message", "La reposición contiene numero inválidos: (" + sol.ToString() + "). Cambio NaN -> cero" }
                        });
                    sol = new Vector3(float.IsNaN(sol.x) ? 0 : sol.x, float.IsNaN(sol.y) ? 0 : sol.y, float.IsNaN(sol.y) ? 0 : sol.y);
                }
                else
                {
                    // Posición final
                    newPin.transform.position = sol;
                }
                #if VISUAL_DEBUG
    				//debug posicion new pin en el momento de la collision
                    DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.white ) );
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
                    DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, Solution1, newPin.GetRadius(), Color.yellow ) );
                    DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution1, Solution1 + (A.GetPosition () - Solution1).normalized * newPin.GetRadius(), Color.yellow ) ); // Linea Solution1-A
                    DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution1, Solution1 + (B.GetPosition () - Solution1).normalized * newPin.GetRadius(), Color.yellow ) ); // Linea Solution1-B
    				//Solution 2
                    DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, Solution2, newPin.GetRadius(), Color.yellow ) );			
                    DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution2, Solution2 + (A.GetPosition () - Solution2).normalized * newPin.GetRadius(), Color.yellow ) ); // Linea Solution2-A
                    DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution2, Solution2 + (B.GetPosition () - Solution2).normalized * newPin.GetRadius(), Color.yellow ) ); // Linea Solution2-B
    				//Posicion final decidida...
    				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.transform.position, newPin.GetRadius(), Color.black ) );
                #endif
                
                break;
            default:				
                AnalyticsSender.SendCustomAnalitycs("wtfError", new Dictionary<string, object>()
                    {
                        { "type", "0004" },
                        { "message", "número de colisiones incorrectas: (" + circumferencesCollided.Count.ToString() + ")" }
                    }
                );
                break;
        }
    }

    float circleDivisions = 360f / 12f;

    Vector3 SnapNormalizedVector(Vector3 newPinPos, Vector3 collidedPinPos)
    {
        Vector3 snappedDirection = SnapToAngle(newPinPos, collidedPinPos);
        return -snappedDirection;
    }

    /// <summary>
    /// Fija un angulo dado a las divisiones de una circunferencia.
    /// </summary>
    /// <returns>The to.</returns>
    /// <param name="v3">V3.</param>
    /// <param name="snapAngle">Snap angle.</param>
    /// <param name="url">http://answers.unity3d.com/questions/493006/snap-a-direction-vector.html</param> 
    Vector3 SnapToAngle(Vector3 newPinPos, Vector3 collidedPinPos)
    {		
        Vector3 axisDirection;

        // If collides with me (rotator) reference vector direction is Vector3.down (0,-1,0)
        if (me.GetPosition() == collidedPinPos)
            axisDirection = Vector3.down;
        // else vector direction reference is Vector direction between me and collided pin
		else
            axisDirection = (collidedPinPos - me.GetPosition()).normalized;
		
        //Vector direction between collided pin and thrown pin
        Vector3 dir = (collidedPinPos - newPinPos).normalized;

        float angle = Vector3.Angle(axisDirection, dir);

        //float originalAngle = angle;

        if (angle <= 110)
        {
            angle = 110;
        }
		
        //final angle result
        float snappedAngle = angle.RoundToNearest(circleDivisions);

        // angle and direction to Vector3
        return new Vector3(Mathf.Sin(snappedAngle * Mathf.Deg2Rad) * Mathf.Sign(dir.x), -Mathf.Cos(snappedAngle * Mathf.Deg2Rad) * Mathf.Sign(dir.y), 0);
    }

    public void ProcessPin(Circumference newCircumference)
    {
        if (circumferencesCollided.Count == 1)
        {
            // If collide with me (rotator) generate new color group
            if (circumferencesCollided[0].tag == "Rotator")
            {
                groupsDictionary.Add(groupsDictionary.Count, new ColorPinsGroup(groupsDictionary.Count, newCircumference));
            }
            else
            {
                // else add to an active color group
                for (int i = 0; i < groupsDictionary.Count; i++)
                {
                    if (groupsDictionary[i].isActive)
                    {
                        if (groupsDictionary[i].Contains(circumferencesCollided[0]))
                        {
                            groupsDictionary[i].AddMember(newCircumference);
                        }
                    }
                }
            }
        }
        else if (circumferencesCollided.Count > 1)
        {
            groupsCollided.Clear();
            for (int i = 0; i < circumferencesCollided.Count && !GameManager.Instance.isGameOver; i++)
            {
                if (circumferencesCollided[i].tag == "Pin")
                {
                    for (int j = 0; j < groupsDictionary.Count; j++)
                    {
                        if (groupsDictionary[j].isActive)
                        {
                            if (groupsDictionary[j].Contains(circumferencesCollided[i]))
                            {
                                if (!groupsCollided.Contains(groupsDictionary[j].index))
                                {
                                    groupsCollided.Add(groupsDictionary[j].index);
                                }
                            }
                        }
                    }
                }
            }

            if (groupsCollided.Count > 0)
            { // ... if found a color group that constains the collided Pin, add the new Pin to it.
                groupsDictionary[groupsCollided[0]].AddMember(newCircumference);
                CombineGroups(groupsCollided);
            }
            else
            { // ... if the Pin collided didn't belong to a group is a WTFError and nees to fix this
                string log = "";
                foreach (var item in circumferencesCollided)
                {
                    log += "\n - " + item.name;
                }

                AnalyticsSender.SendCustomAnalitycs("wtfError", new Dictionary<string, object>()
                    {
                        { "type", "0005" },
                        { "message", string.Format("Los pins colisionados no están en ningún grupo. \nPin Evaluado: {0} - \nPin Colisionados: {1}", newCircumference.name, log) }
                    }
                );
            }
        }
    }

    void CombineGroups(List<int> groupsCollided)
    {
        // If collides with more than 1 group unify them
        if (groupsCollided.Count > 1)
        {
            int destiny = groupsCollided[0];
            for (int i = 1; i < groupsCollided.Count; i++)
            {
                int origin = groupsCollided[i];
                groupsDictionary[destiny].AddMembers(groupsDictionary[origin].members);
                groupsDictionary[origin].CombineWith(destiny);
            }
        }
    }

    float ProcessPinsGroups()
    {	
        int totalPinsToDestroy = 0;	
        // IF the is a group with more than PIN_MEMBERS_PER_GROUP pins, remove the child pins and the group
        for (int i = 0; i < groupsDictionary.Count; i++)
        {
            if (groupsDictionary[i].isActive)
            {
                if (groupsDictionary[i].Count >= PIN_MEMBERS_PER_GROUP)
                {
                    totalPinsToDestroy += groupsDictionary[i].Count;
                    groupsDictionary[i].Erase();
                }
            }
        }
        return Spawner.MINIMUM_SPAWN_TIME;
    }

    public void EraseAllPins()
    {
        foreach (KeyValuePair<int, ColorPinsGroup> pg in groupsDictionary)
            StartCoroutine(pg.Value.DestroyMembers(false));
    }

    float DistanceBetweenPoints(Vector3 A, Vector3 B)
    {
        float d = Mathf.Round((B - A).sqrMagnitude * 100) / 100;
        return d;
    }

    float DistanceBetweenCircumferences(Circumference A, Circumference B)
    {
        float d = DistanceBetweenPoints(A.GetPosition(), B.GetPosition()) - ((A.GetRadius() + B.GetRadius()) * (A.GetRadius() + B.GetRadius()));
        return d;
    }

    bool IsColliding(Circumference A, Circumference B, float margin = 0f)
    {
        bool ret;
        if (A.colorType == B.colorType || B.colorType == -1)
        {
            ret = DistanceBetweenCircumferences(A, B) <= margin;
        }
        else
        {
            ret = DistanceBetweenCircumferences(A, B) <= 0;
        }
        return ret;
    }

    public void Reset()
    {
        circumferencesCollided.Clear();
        GameObject[] pins = GameObject.FindGameObjectsWithTag("Pin");
        for (int i = 0; i < pins.Length; i++)
        {
            Destroy(pins[i]);
        }
        groupsDictionary.Clear();
        StopInverseDirection();
        StopCrazySpeed();
        RotationSpeed = INITIAL_SPEED;
        variableSpeedInc = 0;
        rotationDirection = 1;
    }

    void PlaySound(int id)
    {
        switch (id)
        {
            case 0:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_1);
                break;
            case 1:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_2);
                break;
            case 2:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_3);
                break;
            case 4:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_4);
                break;
            case 5:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_5);
                break;
            case 6:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_6);
                break;
            case 7:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_7);
                break;
            case 8:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_8);
                break;
            case 9:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_9);
                break;
            case 10:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_10);
                break;
            default:
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_1);
                break;
        }
    }

    public void StartInverseDirection()
    {
        canUseCrazySpeed = false;
        canInverseDir = true;
    }

    public void StopInverseDirection()
    {
        canInverseDir = false;
    }

    public void StartCrazySpeed()
    {
        canInverseDir = false;
        canUseCrazySpeed = true;
        StartCoroutine(CrazySpeedDifficult());
    }

    public void StopCrazySpeed()
    {
        StopCoroutine(CrazySpeedDifficult());
        canUseCrazySpeed = false;
        variableSpeedInc = 0;
    }

    public IEnumerator CrazySpeedDifficult()
    {
        while (canUseCrazySpeed && !GameManager.Instance.isGameOver)
        {
            float tmpInc = speedIncs[UnityEngine.Random.Range(0, speedIncs.Length)];
            // if random repeats number, find new one
            while (newCrazySpeedInc == tmpInc)
            {
                tmpInc = speedIncs[UnityEngine.Random.Range(0, speedIncs.Length)];
            }
            newCrazySpeedInc = tmpInc;
            StartCoroutine(SmoothSpeedIncrement(variableSpeedInc, newCrazySpeedInc, 1f));

            yield return new WaitForSeconds((float)UnityEngine.Random.Range(3, 5));
        }
    }

    public IEnumerator SmoothSpeedIncrement(float from, float to, float time)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            variableSpeedInc = Mathf.Lerp(from, to, elapsedTime / time);
            yield return null;
        }
    }

    #if VISUAL_DEBUG
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
    #endif
}