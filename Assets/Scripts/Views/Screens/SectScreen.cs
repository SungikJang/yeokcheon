// Assets/Scripts/Views/Screens/SectScreen.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YeokCheonEngine.ElementSystem.ViewSystem;
using YeokCheonDomain.Sect;

namespace Views.Screens
{
    public sealed class SectScreen : View
    {
        public override void OnSpawn()
        {
            base.OnSpawn();
            SubscribeToState<SectState>(OnSectChanged);
        }

        public override void OnEnableElement()
        {
            base.OnEnableElement();

            // 문파 선택 버튼 3개 연결
            var btnJeong = QueryComponent<Button>("Btn_Jeong"); // 정파
            var btnSa    = QueryComponent<Button>("Btn_Sa");    // 사파
            var btnMa    = QueryComponent<Button>("Btn_Ma");    // 마교

            if (btnJeong) btnJeong.onClick.AddListener(() => JoinSect(Faction.정파, 0));
            if (btnSa)    btnSa.onClick.AddListener(()    => JoinSect(Faction.사파, 3));
            if (btnMa)    btnMa.onClick.AddListener(()    => JoinSect(Faction.마교, 6));
        }

        public override void OnDisableElement()
        {
            base.OnDisableElement();

            var btnJeong = QueryComponent<Button>("Btn_Jeong");
            var btnSa    = QueryComponent<Button>("Btn_Sa");
            var btnMa    = QueryComponent<Button>("Btn_Ma");

            if (btnJeong) btnJeong.onClick.RemoveAllListeners();
            if (btnSa)    btnSa.onClick.RemoveAllListeners();
            if (btnMa)    btnMa.onClick.RemoveAllListeners();
        }

        private void OnSectChanged(SectState state)
        {
            // 미가입: 선택 UI 표시
            var selectPanel = QueryComponent<RectTransform>("Panel_Select");
            var infoPanel   = QueryComponent<RectTransform>("Panel_Info");

            if (selectPanel) selectPanel.gameObject.SetActive(!state.IsInSect);
            if (infoPanel)   infoPanel.gameObject.SetActive(state.IsInSect);

            if (!state.IsInSect) return;

            // 가입 후: 문파 정보 표시
            var factionText = QueryComponent<TextMeshProUGUI>("Text_Faction");
            var gradeText   = QueryComponent<TextMeshProUGUI>("Text_Grade");

            if (factionText) factionText.text = state.Faction.ToString();
            if (gradeText)   gradeText.text   = $"등급: {state.SectGrade}";
        }

        private void JoinSect(Faction faction, int sectId)
        {
            GlobalEngine.Dispatch<SectState>(new JoinSectTrigger
            {
                Faction = faction,
                SectId  = sectId,
            });
        }
    }
}