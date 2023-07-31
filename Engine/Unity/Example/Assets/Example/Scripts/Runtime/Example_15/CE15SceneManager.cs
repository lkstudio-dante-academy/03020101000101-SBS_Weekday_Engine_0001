using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/** Example 15 */
public class CE15SceneManager : CSceneManager {
	/** 상태 */
	public enum EState {
		NONE = -1,
		PLAY,
		GAME_OVER,
		[HideInInspector] MAX_VAL
	}

	#region 변수
	private EState m_eState = EState.PLAY;
	[SerializeField] private CE15Player m_oPlayer = null;

	[SerializeField] private GameObject m_oBulletRoot = null;
	[SerializeField] private GameObject m_oOriginBullet = null;

	[SerializeField] private GameObject m_oMonsterRoot = null;
	[SerializeField] private GameObject m_oOriginMonster = null;

	[SerializeField] private List<GameObject> m_oMonsterSpawnPosList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public override string SceneName => KDefine.G_SCENE_N_E15;
	public CE15Player Player => m_oPlayer;

	public CE15ObjsPoolManager ObjsPoolManager { get; private set; } = null;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public override void Awake() {
		base.Awake();

		this.ObjsPoolManager = CFactory.CreateGameObj<CE15ObjsPoolManager>("ObjsPoolManager",
			this.gameObject, Vector3.zero, Vector3.one, Vector3.zero);
	}

	/** 초기화 */
	public override void Start() {
		base.Start();
		StartCoroutine(this.TryCreateMonsters());
	}

	/** 상태를 갱신한다 */
	public override void Update() {
		base.Update();

		// 스페이스 키를 눌렀을 경우
		if(Input.GetKeyDown(KeyCode.Space)) {
			m_oPlayer.Fire(m_oOriginBullet, m_oBulletRoot);
		}
	}

	/** 몬스터 생성을 시도한다 */
	private IEnumerator TryCreateMonsters() {
		do {
			var oMonster = (this.ObjsPoolManager.SpawnObj<CE15Monster>(() => {
				return CFactory.CreateCloneGameObj("Monster",
					m_oOriginMonster, m_oMonsterRoot, Vector3.zero, Vector3.one, Vector3.zero);
			}) as GameObject).GetComponent<CE15Monster>();

			int nPosIdx = Random.Range(0, m_oMonsterSpawnPosList.Count);
			int nWalkable = NavMesh.GetAreaFromName("Walkable");

			var oSpawnPos = m_oMonsterSpawnPosList[nPosIdx];

			// 위치를 계산했을 경우
			if(NavMesh.SamplePosition(oSpawnPos.transform.position,
				out NavMeshHit stNavMeshHit, float.MaxValue, 1 << nWalkable)) {
				oMonster.transform.position = stNavMeshHit.position;
			}

			oMonster.Init();
			oMonster.gameObject.SetActive(true);

			yield return new WaitForSeconds(5.0f);
		} while(m_eState != EState.GAME_OVER);
	}
	#endregion // 함수
}
