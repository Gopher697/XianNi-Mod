using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace xn.bloodline
{
    public class UnitBloodlineElement : UnitElement
    {
        private const int AVATARS_LIMIT_PER_UNFOLD = 128;
        private const int AVATARS_LIMIT_INITIAL = 16;
        public const float COUNT_ANIMATION_STEP_TIME = 0.025f;
        public UiUnitAvatarElement prefab_avatar;
        public UnfoldButton _prefab_unfolder;
        public Image _sex_icon;
        public Sprite _default_bloodline_icon;
        private UnfoldButton _members_unfolder;
        private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_founder;
        private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_elders;
        private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_enforcers;
        private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_members;
        public Transform transform_founder;   
        public Transform transform_elders;    
        public Transform transform_enforcers; 
        public Transform transform_members;   
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
            if (transform_founder == null)
                transform_founder = transform.Find("Grandparents");
            if (transform_elders == null)
                transform_elders = transform.Find("Parents");
            if (transform_enforcers == null)
                transform_enforcers = transform.Find("Siblings");
            if (transform_members == null)
                transform_members = transform.Find("Children");
            SetLocalizedText(transform_founder, "bloodline_founder");
            SetLocalizedText(transform_elders, "bloodline_elders");
            SetLocalizedText(transform_enforcers, "bloodline_inner_disciples");
            SetLocalizedText(transform_members, "bloodline_outer_disciples");
            if (prefab_avatar != null)
            {
                if (_pool_founder == null && transform_founder != null)
                    _pool_founder = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_founder);
                if (_pool_elders == null && transform_elders != null)
                    _pool_elders = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_elders);
                if (_pool_enforcers == null && transform_enforcers != null)
                    _pool_enforcers = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_enforcers);
                if (_pool_members == null && transform_members != null)
                    _pool_members = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_members);
            }
            if (_prefab_unfolder != null)
            {
                if (_members_unfolder == null && transform_members != null)
                {
                    _members_unfolder = Object.Instantiate(_prefab_unfolder, transform_members);
                    _members_unfolder.setCallback(delegate
                    {
                        StartCoroutine(loadOuterDisciples(pUnfold: true));
                    });
                }
            }
            var tab_title_container_obj = transform.Find("tab_title_container_unit");
            if (tab_title_container_obj != null)
            {
                var localizedText = tab_title_container_obj.GetComponentInChildren<LocalizedText>(true);
                if (localizedText != null)
                {
                    localizedText.key = "tab_bloodline";
                }
            }
        }
        public override IEnumerator showContent()
        {
            SetLocalizedText(transform_founder, "bloodline_founder");
            SetLocalizedText(transform_elders, "bloodline_elders");
            SetLocalizedText(transform_enforcers, "bloodline_inner_disciples");
            SetLocalizedText(transform_members, "bloodline_outer_disciples");
            if (_sex_icon != null)
            {
                _sex_icon.sprite = _default_bloodline_icon ?? SpriteTextureLoader.getSprite("ui/icon/bloodline");
            }
            if (!BloodlineSystem.HasBloodline(actor))
            {
                yield break;
            }
            yield return loadFounder();
            yield return loadElders();
            yield return loadInnerDisciples();
            yield return loadOuterDisciples();
        }
        public override void clear()
        {
            if (_pool_founder != null) _pool_founder.clear();
            if (_pool_elders != null) _pool_elders.clear();
            if (_pool_enforcers != null) _pool_enforcers.clear();
            if (_pool_members != null) _pool_members.clear();
            if (prefab_avatar != null) prefab_avatar.gameObject.SetActive(false);
            if (_members_unfolder != null)
            {
                _members_unfolder.gameObject.SetActive(false);
                _members_unfolder.clear();
            }
            base.clear();
        }
        private IEnumerator loadFounder()
        {
            if (_pool_founder == null) yield break;
            long founderId = BloodlineSystem.GetFounderId(actor);
            if (founderId <= 0)
            {
                if (BloodlineSystem.IsFounder(actor))
                {
                    founderId = actor.getID();
                }
                else
                {
                    yield break;
                }
            }
            var founder = World.world.units.get(founderId);
            if (founder != null && !founder.isRekt())
            {
                track_objects.Add(founder);
                yield return showAvatar(founder, _pool_founder);
            }
            var allMembers = BloodlineSystem.GetBloodlineDescendants(founderId);
            if (allMembers != null && allMembers.Count > 0)
            {
                var atavists = new List<Actor>();
                foreach (var member in allMembers)
                {
                    if (member == null || member.isRekt()) continue;
                    if (BloodlineSystem.IsFounder(member)) continue; 
                    if (BloodlineSystem.IsAtavism(member))
                    {
                        atavists.Add(member);
                    }
                }
                atavists.Sort((a, b) =>
                {
                    int genA = BloodlineSystem.GetGeneration(a);
                    int genB = BloodlineSystem.GetGeneration(b);
                    return genA.CompareTo(genB);
                });
                foreach (var atavist in atavists)
                {
                    track_objects.Add(atavist);
                    yield return showAvatar(atavist, _pool_founder);
                }
            }
        }
        private IEnumerator loadElders()
        {
            if (_pool_elders == null) yield break;
            long founderId = BloodlineSystem.IsFounder(actor) ? actor.getID() : BloodlineSystem.GetFounderId(actor);
            if (founderId <= 0) yield break;
            var allMembers = BloodlineSystem.GetBloodlineDescendants(founderId);
            if (allMembers == null || allMembers.Count == 0) yield break;
            var sortedMembers = new List<Actor>();
            foreach (var member in allMembers)
            {
                if (member == null || member.isRekt()) continue;
                if (member.getID() == actor.getID()) continue;
                if (BloodlineSystem.IsFounder(member)) continue;
                if (BloodlineSystem.IsAtavism(member)) continue; 
                sortedMembers.Add(member);
            }
            if (sortedMembers.Count == 0) yield break;
            sortedMembers.Sort((a, b) =>
            {
                int realmA = GetRealmLevel(a);
                int realmB = GetRealmLevel(b);
                if (realmA != realmB)
                    return realmB.CompareTo(realmA);
                float concA = BloodlineSystem.GetConcentration(a);
                float concB = BloodlineSystem.GetConcentration(b);
                return concB.CompareTo(concA);
            });
            int elderCount = Mathf.Min(9, sortedMembers.Count);
            var elders = sortedMembers.GetRange(0, elderCount);
            track_objects.AddRange(elders);
            foreach (var elder in elders)
            {
                yield return showAvatar(elder, _pool_elders);
            }
        }
        private IEnumerator loadInnerDisciples()
        {
            if (_pool_enforcers == null) yield break;
            long founderId = BloodlineSystem.IsFounder(actor) ? actor.getID() : BloodlineSystem.GetFounderId(actor);
            if (founderId <= 0) yield break;
            var allMembers = BloodlineSystem.GetBloodlineDescendants(founderId);
            if (allMembers == null || allMembers.Count == 0) yield break;
            var sortedMembers = new List<Actor>();
            foreach (var member in allMembers)
            {
                if (member == null || member.isRekt()) continue;
                if (member.getID() == actor.getID()) continue;
                if (BloodlineSystem.IsFounder(member)) continue;
                if (BloodlineSystem.IsAtavism(member)) continue;
                float conc = BloodlineSystem.GetConcentration(member);
                if (conc > 20f)
                {
                    sortedMembers.Add(member);
                }
            }
            if (sortedMembers.Count == 0) yield break;
            sortedMembers.Sort((a, b) =>
            {
                int realmA = GetRealmLevel(a);
                int realmB = GetRealmLevel(b);
                if (realmA != realmB)
                    return realmB.CompareTo(realmA);
                float concA = BloodlineSystem.GetConcentration(a);
                float concB = BloodlineSystem.GetConcentration(b);
                return concB.CompareTo(concA);
            });
            if (sortedMembers.Count <= 9) yield break;
            var innerDisciples = sortedMembers.GetRange(9, sortedMembers.Count - 9);
            track_objects.AddRange(innerDisciples);
            foreach (var disciple in innerDisciples)
            {
                yield return showAvatar(disciple, _pool_enforcers);
            }
        }
        private IEnumerator loadOuterDisciples(bool pUnfold = false)
        {
            if (_pool_members == null) yield break;
            if (_members_unfolder != null)
            {
                _members_unfolder.gameObject.SetActive(false);
            }
            long founderId = BloodlineSystem.IsFounder(actor) ? actor.getID() : BloodlineSystem.GetFounderId(actor);
            if (founderId <= 0) yield break;
            var allMembers = BloodlineSystem.GetBloodlineDescendants(founderId);
            if (allMembers == null || allMembers.Count == 0) yield break;
            var outerDisciples = new List<Actor>();
            foreach (var member in allMembers)
            {
                if (member == null || member.isRekt()) continue;
                if (member.getID() == actor.getID()) continue;
                if (BloodlineSystem.IsFounder(member)) continue;
                if (BloodlineSystem.IsAtavism(member)) continue;
                float conc = BloodlineSystem.GetConcentration(member);
                if (conc <= 20f)
                {
                    outerDisciples.Add(member);
                }
            }
            if (outerDisciples.Count == 0) yield break;
            outerDisciples.Sort((a, b) =>
            {
                float concA = BloodlineSystem.GetConcentration(a);
                float concB = BloodlineSystem.GetConcentration(b);
                return concB.CompareTo(concA);
            });
            track_objects.AddRange(outerDisciples);
            int tAvatarsLimit = pUnfold ? AVATARS_LIMIT_PER_UNFOLD : AVATARS_LIMIT_INITIAL;
            int tIndex = 0;
            int tLeft = 0;
            int tLatestShown = 0;
            using ListPool<Actor> tFiltered = new ListPool<Actor>();
            foreach (Actor tActor in outerDisciples)
            {
                tIndex++;
                if (_members_unfolder == null || _members_unfolder.offset == 0 || tIndex >= _members_unfolder.offset)
                {
                    if (_members_unfolder != null && tIndex - _members_unfolder.offset >= tAvatarsLimit)
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
                yield return showAvatar(tActor, _pool_members);
            }
            if (_members_unfolder != null && tLeft > 0)
            {
                _members_unfolder.transform.SetSiblingIndex(transform_members.childCount - 1);
                _members_unfolder.gameObject.SetActive(true);
                _members_unfolder.setData(tLeft, tLatestShown);
                StartCoroutine(counter(tLeft, _members_unfolder));
            }
        }
        private int GetRealmLevel(Actor a)
        {
            int realmIdx = BloodlineSystem.GetRealmIndex(a);
            if (realmIdx >= 0)
                return realmIdx + 100;
            int ancStar = BloodlineSystem.GetAncientStar(a);
            if (ancStar > 0)
                return ancStar + 50;
            int beastStage = BloodlineSystem.GetBeastStage(a);
            if (beastStage > 0)
                return beastStage;
            return 0;
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