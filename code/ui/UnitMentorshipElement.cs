using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace xn.ui
{
    public class UnitMentorshipElement : UnitElement
    {
        private const int AVATARS_LIMIT_PER_UNFOLD = 128;
        private const int AVATARS_LIMIT_INITIAL = 16;
        public const float COUNT_ANIMATION_STEP_TIME = 0.025f;
        public UiUnitAvatarElement prefab_avatar;
        public UnfoldButton _prefab_unfolder;
        public Image _sex_icon;
        public Sprite _default_mentorship_icon;
        private UnfoldButton _tongmen_unfolder;
        private UnfoldButton _disciples_unfolder;
        private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_shizu;
        private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_shifu;
        private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_tongmen;
        private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_disciples;
        public Transform transform_shizu;
        public Transform transform_shifu;
        public Transform transform_tongmen;
        public Transform transform_disciples;
        private const string KEY_MASTER_ID = "xn_men_master_id";
        private const string KEY_DISCIPLES_IDS = "xn_men_disciples_ids";
        public override void Awake()
        {
            base.Awake();
        }
        public override void OnEnable()
        {
            EnsureInitialized();
            base.OnEnable();
        }
        private void EnsureInitialized()
        {
            if (prefab_avatar == null)
            {
            prefab_avatar = GetComponentInChildren<UiUnitAvatarElement>(true);
            }
            if (prefab_avatar != null && !prefab_avatar.gameObject.activeSelf)
            {
                prefab_avatar.gameObject.SetActive(false);
            }
            if (transform_shizu == null)
                {
                transform_shizu = transform.Find("Grandparents");
            }
            if (transform_shifu == null)
            {
                transform_shifu = transform.Find("Parents");
            }
            if (transform_tongmen == null)
            {
                transform_tongmen = transform.Find("Siblings");
            }
            if (transform_disciples == null)
            {
                transform_disciples = transform.Find("Children");
            }
            SetLocalizedText(transform_shizu, "mentorship_grandparents");
            SetLocalizedText(transform_shifu, "mentorship_parents");
            SetLocalizedText(transform_tongmen, "mentorship_siblings");
            SetLocalizedText(transform_disciples, "mentorship_children");
            if (prefab_avatar != null)
            {
                if (_pool_shizu == null && transform_shizu != null)
            _pool_shizu = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_shizu);
                if (_pool_shifu == null && transform_shifu != null)
            _pool_shifu = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_shifu);
                if (_pool_tongmen == null && transform_tongmen != null)
            _pool_tongmen = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_tongmen);
                if (_pool_disciples == null && transform_disciples != null)
            _pool_disciples = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_disciples);
            }
            if (_prefab_unfolder != null)
            {
                if (_tongmen_unfolder == null && transform_tongmen != null)
            {
                _tongmen_unfolder = Object.Instantiate(_prefab_unfolder, transform_tongmen);
                _tongmen_unfolder.setCallback(delegate
                {
                    StartCoroutine(loadTongmen(pUnfold: true));
                });
                }
                if (_disciples_unfolder == null && transform_disciples != null)
                {
                _disciples_unfolder = Object.Instantiate(_prefab_unfolder, transform_disciples);
                _disciples_unfolder.setCallback(delegate
                {
                    StartCoroutine(loadDisciples(pUnfold: true));
                });
            }
        }
            var tab_title_container_obj = transform.Find("tab_title_container_unit");
            if (tab_title_container_obj != null)
        {
                var localizedText = tab_title_container_obj.GetComponentInChildren<LocalizedText>(true);
                if (localizedText != null)
                {
                    localizedText.key = "tab_mentorship";
                }
            }
        }
        public override IEnumerator showContent()
        {
            SetLocalizedText(transform_shizu, "mentorship_grandparents");
            SetLocalizedText(transform_shifu, "mentorship_parents");
            SetLocalizedText(transform_tongmen, "mentorship_siblings");
            SetLocalizedText(transform_disciples, "mentorship_children");
            if (actor.asset.inspect_sex)
            {
                if (actor.isSexMale())
                {
                    _sex_icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconMale");
                }
                else
                {
                    _sex_icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconFemale");
                }
            }
            else
            {
                _sex_icon.sprite = _default_mentorship_icon;
            }
            yield return loadShizu();
            yield return loadShifu();
            yield return loadTongmen();
            yield return loadDisciples();
        }
        public override void clear()
        {
            if (_pool_shizu != null) _pool_shizu.clear();
            if (_pool_shifu != null) _pool_shifu.clear();
            if (_pool_tongmen != null) _pool_tongmen.clear();
            if (_pool_disciples != null) _pool_disciples.clear();
            if (prefab_avatar != null) prefab_avatar.gameObject.SetActive(value: false);
            if (_tongmen_unfolder != null)
            {
                _tongmen_unfolder.gameObject.SetActive(value: false);
                _tongmen_unfolder.clear();
            }
            if (_disciples_unfolder != null)
            {
                _disciples_unfolder.gameObject.SetActive(value: false);
                _disciples_unfolder.clear();
            }
            base.clear();
        }
        private IEnumerator loadShizu()
        {
            if (_pool_shizu == null) yield break;
            long masterId;
            actor.data.get(KEY_MASTER_ID, out masterId, 0L);
            if (masterId <= 0) yield break;
            var shifu = World.world.units.get(masterId);
            if (shifu == null || shifu.isRekt()) yield break;
            long shizuId;
            shifu.data.get(KEY_MASTER_ID, out shizuId, 0L);
            if (shizuId <= 0) yield break;
            var shizu = World.world.units.get(shizuId);
            if (shizu == null || shizu.isRekt()) yield break;
            track_objects.Add(shizu);
            yield return showAvatar(shizu, _pool_shizu);
        }
        private IEnumerator loadShifu()
        {
            if (_pool_shifu == null) yield break;
            long masterId;
            actor.data.get(KEY_MASTER_ID, out masterId, 0L);
            if (masterId <= 0) yield break;
            var shifu = World.world.units.get(masterId);
            if (shifu == null || shifu.isRekt()) yield break;
            track_objects.Add(shifu);
            yield return showAvatar(shifu, _pool_shifu);
        }
        private IEnumerator loadTongmen(bool pUnfold = false)
        {
            if (_pool_tongmen == null) yield break;
            if (_tongmen_unfolder != null)
            {
                _tongmen_unfolder.gameObject.SetActive(false);
        }
            long myMasterId;
            actor.data.get(KEY_MASTER_ID, out myMasterId, 0L);
            if (myMasterId <= 0) yield break;
            var master = World.world.units.get(myMasterId);
            if (master == null || master.isRekt()) yield break;
            string idsStr;
            master.data.get(KEY_DISCIPLES_IDS, out idsStr, "");
            if (string.IsNullOrEmpty(idsStr)) yield break;
            using ListPool<Actor> tTongmen = new ListPool<Actor>();
            string[] parts = idsStr.Split(',');
            foreach (var part in parts)
            {
                if (long.TryParse(part.Trim(), out long id) && id > 0)
                {
                    var disc = World.world.units.get(id);
                    if (disc != null && !disc.isRekt() && disc.data.id != actor.data.id)
        {
                        tTongmen.Add(disc);
                    }
                }
            }
            if (tTongmen.Count == 0) yield break;
            track_objects.AddRange(tTongmen);
            int tAvatarsLimit = (pUnfold ? AVATARS_LIMIT_PER_UNFOLD : AVATARS_LIMIT_INITIAL);
            int tIndex = 0;
            int tLeft = 0;
            int tLatestShown = 0;
            using ListPool<Actor> tFiltered = new ListPool<Actor>();
            foreach (Actor tActor in tTongmen)
            {
                tIndex++;
                if (_tongmen_unfolder == null || _tongmen_unfolder.offset == 0 || tIndex >= _tongmen_unfolder.offset)
            {
                    if (_tongmen_unfolder != null && tIndex - _tongmen_unfolder.offset >= tAvatarsLimit)
                {
                        tLeft++;
                    continue;
                }
                    tLatestShown = tIndex;
                    tFiltered.Add(tActor);
                }
            }
            foreach (Actor tActor in tFiltered)
            {
                yield return showAvatar(tActor, _pool_tongmen);
            }
            if (_tongmen_unfolder != null && tLeft > 0)
            {
                _tongmen_unfolder.transform.SetSiblingIndex(transform_tongmen.childCount - 1);
                _tongmen_unfolder.gameObject.SetActive(true);
                _tongmen_unfolder.setData(tLeft, tLatestShown);
                StartCoroutine(counter(tLeft, _tongmen_unfolder));
            }
        }
        private IEnumerator loadDisciples(bool pUnfold = false)
        {
            if (_pool_disciples == null) yield break;
            if (_disciples_unfolder != null)
            {
                _disciples_unfolder.gameObject.SetActive(false);
            }
            string idsStr;
            actor.data.get(KEY_DISCIPLES_IDS, out idsStr, "");
            if (string.IsNullOrEmpty(idsStr)) yield break;
            using ListPool<Actor> tDisciples = new ListPool<Actor>();
            string[] parts = idsStr.Split(',');
            foreach (var part in parts)
            {
                if (long.TryParse(part.Trim(), out long id) && id > 0)
                {
                    var disc = World.world.units.get(id);
                    if (disc != null && !disc.isRekt())
                    {
                        tDisciples.Add(disc);
                    }
                }
            }
            if (tDisciples.Count == 0) yield break;
            track_objects.AddRange(tDisciples);
            int tAvatarsLimit = (pUnfold ? AVATARS_LIMIT_PER_UNFOLD : AVATARS_LIMIT_INITIAL);
            int tIndex = 0;
            int tLeft = 0;
            int tLatestShown = 0;
            using ListPool<Actor> tFiltered = new ListPool<Actor>();
            foreach (Actor tActor in tDisciples)
            {
                tIndex++;
                if (_disciples_unfolder == null || _disciples_unfolder.offset == 0 || tIndex >= _disciples_unfolder.offset)
                {
                    if (_disciples_unfolder != null && tIndex - _disciples_unfolder.offset >= tAvatarsLimit)
                    {
                        tLeft++;
                        continue;
                    }
                    tLatestShown = tIndex;
                    tFiltered.Add(tActor);
                }
            }
            foreach (Actor tActor in tFiltered)
            {
                yield return showAvatar(tActor, _pool_disciples);
            }
            if (_disciples_unfolder != null && tLeft > 0)
            {
                _disciples_unfolder.transform.SetSiblingIndex(transform_disciples.childCount - 1);
                _disciples_unfolder.gameObject.SetActive(true);
                _disciples_unfolder.setData(tLeft, tLatestShown);
                StartCoroutine(counter(tLeft, _disciples_unfolder));
            }
        }
        private IEnumerator counter(int pLeft, UnfoldButton pButton)
        {
            float tPerStep = (float)pLeft / 20f;
            for (float i = 0f; i < (float)(pLeft + 1); i += tPerStep)
            {
                string tText = "+" + Mathf.Floor(i);
                pButton.setText(tText);
                yield return new WaitForSecondsRealtime(COUNT_ANIMATION_STEP_TIME);
            }
        }
        private IEnumerator showAvatar(Actor pActor, ObjectPoolGenericMono<UiUnitAvatarElement> pPool)
        {
            if (!pActor.isRekt())
            {
                yield return new WaitForSecondsRealtime(COUNT_ANIMATION_STEP_TIME);
                if (!pActor.isRekt())
                {
                    pPool.getNext().show(pActor);
                }
            }
        }
        private delegate bool UnfoldCheck(Actor pActor);
        private void SetLocalizedText(Transform parent, string key)
        {
            if (parent == null) return;
            var localizedText = parent.GetComponentInChildren<LocalizedText>(true);
            if (localizedText != null)
            {
                localizedText.key = key;
                return;
            }
            var textComponent = parent.GetComponentInChildren<Text>(true);
            if (textComponent != null)
            {
                localizedText = textComponent.GetComponent<LocalizedText>();
                if (localizedText == null)
                {
                    localizedText = textComponent.gameObject.AddComponent<LocalizedText>();
                }
                localizedText.key = key;
            }
        }
    }
}