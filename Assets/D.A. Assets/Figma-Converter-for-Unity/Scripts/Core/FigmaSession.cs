using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

#if JSONNET_EXISTS
using Newtonsoft.Json;
#endif

#pragma warning disable IDE0003

namespace DA_Assets.FCU
{
    [Serializable]
    public class FigmaSession : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public FigmaSessionItem CurrentSession { get; set; }
        public string Token => this.CurrentSession.AuthResult.AccessToken;

        public bool IsAuthed()
        {
            if (this.CurrentSession.User.Name.IsEmpty() || this.Token.IsEmpty())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void AddNew(AuthResult authResult)
        {
            GetCurrentFigmaUser(authResult.AccessToken, result =>
            {
                if (result.Success)
                {
                    FigmaSessionItem newSess = new FigmaSessionItem
                    {
                        User = result.Object,
                        AuthResult = authResult
                    };

                    SetLastSession(newSess);

                    DALogger.LogSuccess(FcuLocKey.log_auth_complete.Localize());
                }
                else
                {
                    DALogger.LogError(FcuLocKey.log_cant_auth.Localize(result.Error.Status, result.Error.Message, result.Error.Exception));
                }
            }).StartDARoutine(monoBeh);
        }

        private IEnumerator GetCurrentFigmaUser(string token, Return<FigmaUser> @return)
        {
            DARequest request = new DARequest
            {
                Query = "https://api.figma.com/v1/me",
                RequestType = RequestType.Get,
                RequestHeader = new RequestHeader
                {
                    Name = "Authorization",
                    Value = $"Bearer {token}"
                }
            };

            yield return monoBeh.RequestSender.SendRequest(request, @return);
        }


        public void TryRestoreSession()
        {
            if (monoBeh.IsJsonNetExists() == false)
                return;

            if (IsAuthed() == false)
            {
                FigmaSessionItem item = GetLastSessionItem();
                this.CurrentSession = item;
            }
        }

        private void SetLastSession(FigmaSessionItem sessionItem)
        {
            this.CurrentSession = sessionItem;

            List<FigmaSessionItem> sessionItems = GetSessionItems();

            FigmaSessionItem targetItem = sessionItems.FirstOrDefault(item => item.AuthResult.AccessToken == sessionItem.AuthResult.AccessToken);
            sessionItems.Remove(targetItem);
            sessionItems.Insert(0, sessionItem);

            if (sessionItems.Count > FcuConfig.Instance.FigmaSessionsLimit)
            {
                sessionItems = sessionItems.Take(FcuConfig.Instance.FigmaSessionsLimit).ToList();
            }

            SaveDataToPrefs(sessionItems);
        }


        private FigmaSessionItem GetLastSessionItem()
        {
            List<FigmaSessionItem> items = GetSessionItems();
            return items.FirstOrDefault();
        }

        public List<FigmaSessionItem> GetSessionItems()
        {
            string json = "";
#if UNITY_EDITOR
            json = EditorPrefs.GetString(FcuConfig.Instance.FigmaSessionsPrefsKey, "");
#endif
            List<FigmaSessionItem> sessionItems = new List<FigmaSessionItem>();

            if (json.IsEmpty())
                return sessionItems;

            try
            {
                sessionItems = DAJson.FromJson<List<FigmaSessionItem>>(json);
            }
            catch
            {

            }

            return sessionItems;
        }

        private void SaveDataToPrefs(List<FigmaSessionItem> sessionItems)
        {
#if UNITY_EDITOR && JSONNET_EXISTS
            string json = JsonConvert.SerializeObject(sessionItems);
            EditorPrefs.SetString(FcuConfig.Instance.FigmaSessionsPrefsKey, json);
#endif
        }
    }

    public struct FigmaSessionItem
    {
        public AuthResult AuthResult { get; set; }
        public FigmaUser User { get; set; }
    }
}
