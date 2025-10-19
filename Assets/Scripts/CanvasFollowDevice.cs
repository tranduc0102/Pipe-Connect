using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.UI.Scripts.CanvasPopup
{
    public class CanvasFollowDevice : MonoBehaviour
    {
        [Header("COMPONENT")] [SerializeField] private RectTransform mainRect;
        [SerializeField] private CanvasScaler scaler;
        [SerializeField] private Camera cam;
        [SerializeField] private float aspect;
        [SerializeField] private float refAspect;
        [SerializeField] private bool fatCanvas;
        [SerializeField] private bool postEvent;

        /// Aspect cam theo invert
        public float AspectCam;

        public CanvasScaler Scaler => scaler;
        public float Aspect => aspect;

        /// Có đổi cam size không?
        [Header("CONFIG")] public bool changeCamSize;

        public int defaultCamSize2D = 10;
        public int defaultCamSize3D = 60;
        public float multiplierCamSize;

        /// có đổi canvas scaler ko?
        public bool changeCanvasScaler;

        bool IsInvert;
#pragma warning disable CS0414
        float lastAspect = 0;
#pragma warning restore CS0414

        [Space] [Header("CAM SIZE")] public List<ResolutionInfor> Resolutions = new List<ResolutionInfor>
        {
            new ResolutionInfor
            {
                Name = "Square Phone",
                AspectCam = 1f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 1f
            },
            new ResolutionInfor
            {
                Name = "Fold 2 5G Tablet",
                AspectCam = 2208f / 1768f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 1f
            },
            new ResolutionInfor
            {
                Name = "Ipad",
                AspectCam = 2732f / 2048f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 1f
            },
            new ResolutionInfor
            {
                Name = "Iphone 7",
                AspectCam = 2208f / 1242f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 0.5f
            },
            new ResolutionInfor
            {
                Name = "Iphone XS Max",
                AspectCam = 2437f / 1125f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 0.3f
            },
            new ResolutionInfor
            {
                Name = "Redmi Note 10s",
                AspectCam = 2400f / 1080f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 0.2f
            },
            new ResolutionInfor
            {
                Name = "Z Flip 3",
                AspectCam = 2640f / 1080f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 0.2f
            },
            new ResolutionInfor
            {
                Name = "Fold2 5G Phone",
                AspectCam = 2658f / 960f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 0f
            },
            new ResolutionInfor
            {
                Name = "Long Phone",
                AspectCam = 3f,
                CamSize = 5f,
                PerspectiveSize = 60f,
                Match = 0f
            }
        };

        private void Start()
        {
            ChangeAll();
        }

        [ContextMenu("Change All")]
        public void ChangeAll()
        {
            if (!mainRect)
            {
                mainRect = GetComponent<RectTransform>();
            }

            if (!scaler)
            {
                scaler = GetComponent<CanvasScaler>();
            }

            if (!cam)
            {
                cam = Camera.main;
            }

#if UNITY_EDITOR
            if (cam)
            {
                aspect = cam.pixelRect.width / cam.pixelRect.height;
            }
            else
            {
                aspect = (float)Screen.width / Screen.height;
            }
#else
            aspect = (float)Screen.width / Screen.height;
#endif
            IsInvert = aspect < 1f;
#if UNITY_LUNA
            if(lastAspect != aspect){
                lastAspect = aspect;
                if (IsInvert) 
                    this.PostEvent(EventID.OnPortraitChange);
                else 
                    this.PostEvent(EventID.OnLandcrapeChange);
            }
#endif
            if (scaler)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

                if (IsInvert)
                {
                    scaler.referenceResolution = new Vector2(1080f, 1920f);
                }
                else
                {
                    scaler.referenceResolution = new Vector2(1920f, 1080f);
                }
            }

            refAspect = scaler.referenceResolution.x / scaler.referenceResolution.y;

            ChangeScaler();
            FixCamSizeFollowScreen();
        }


        public void ChangeScaler()
        {
            if (aspect < refAspect)
            {
                scaler.matchWidthOrHeight = 0;
            }
            else
            {
                scaler.matchWidthOrHeight = 1;
            }

            if (fatCanvas)
            {
                scaler.matchWidthOrHeight = 1 - scaler.matchWidthOrHeight;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (cam)
            {
                if (Math.Abs(cam.aspect - aspect) > 0.01f)
                {
                    ChangeAll();
                }
            }
            else
            {
                ChangeAll();
            }
        }
#if !UNITY_LUNA
        private void OnValidate()
        {
            ChangeAll();
        }

        private void OnDrawGizmos()
        {
            if (cam)
            {
                if (Math.Abs(cam.aspect - aspect) > 0.01f)
                {
                    ChangeAll();
                }
            }
            else
            {
                ChangeAll();
            }
        }

        private void OnGUI()
        {
            if (cam)
            {
                if (Math.Abs(cam.aspect - aspect) > 0.01f)
                {
                    ChangeAll();
                }
            }
            else
            {
                ChangeAll();
            }
        }
#endif
        [ContextMenu("Fix cam size follow screen")]
        private void FixCamSizeFollowScreen()
        {
            if ((this && !enabled) || !cam)
                return;

            if (IsInvert)
            {
                AspectCam = 1 / cam.aspect;
            }
            else
            {
                AspectCam = cam.aspect;
            }

            for (int i = 0; i < Resolutions.Count; i++)
            {
                if (Mathf.Abs(AspectCam - Resolutions[i].AspectCam) < 0.05f)
                {
                    if (cam)
                    {
                        if (cam.orthographic)
                        {
                            var camSize = Resolutions[i].CamSize;
                            multiplierCamSize = camSize / defaultCamSize2D;
                            if (changeCamSize)
                            {
                                cam.orthographicSize = camSize;
                            }
                        }
                        else
                        {
                            var camSize = Resolutions[i].PerspectiveSize;
                            multiplierCamSize = camSize / defaultCamSize3D;

                            if (changeCamSize)
                            {
                                cam.fieldOfView = camSize;
                            }
                        }
                    }

                    if (changeCanvasScaler && scaler)
                    {
                        scaler.matchWidthOrHeight = Resolutions[i].Match;
                    }

                    break;
                }
                else
                {
                    int maxIndex = Mathf.Clamp(i + 1, 0, Resolutions.Count - 1);
                    if (AspectCam > Resolutions[i].AspectCam && AspectCam < Resolutions[maxIndex].AspectCam)
                    {
                        if (cam)
                        {
                            if (cam.orthographic)
                            {
                                var camSize = Resolutions[i].CamSize +
                                              (AspectCam - Resolutions[i].AspectCam) /
                                              (Resolutions[maxIndex].AspectCam -
                                               Resolutions[i].AspectCam) *
                                              (Resolutions[maxIndex].CamSize - Resolutions[i].CamSize);
                                multiplierCamSize = camSize / defaultCamSize2D;

                                if (changeCamSize)
                                {
                                    cam.orthographicSize = camSize;
                                }
                            }
                            else
                            {
                                var camSize = Resolutions[i].PerspectiveSize +
                                              (AspectCam - Resolutions[i].AspectCam) /
                                              (Resolutions[maxIndex].AspectCam - Resolutions[i].AspectCam) *
                                              (Resolutions[maxIndex].PerspectiveSize -
                                               Resolutions[i].PerspectiveSize);

                                multiplierCamSize = camSize / defaultCamSize3D;

                                if (changeCamSize)
                                {
                                    cam.fieldOfView = camSize;
                                }
                            }
                        }

                        if (changeCanvasScaler && scaler)
                        {
                            scaler.matchWidthOrHeight = Resolutions[i].Match;
                        }
                    }
                }
            }

            if (Application.isPlaying)
            {
            }
        }
    }

    [Serializable]
    public class ResolutionInfor
    {
        public string Name;
        public float AspectCam;

        /// 2D size
        public float CamSize = 10;

        /// 3D size
        public float PerspectiveSize = 60;

        /// canvas scaler
        [Range(0f, 1f)] public float Match = 0.5f;
    }
}