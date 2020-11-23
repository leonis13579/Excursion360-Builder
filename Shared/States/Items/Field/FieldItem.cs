﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Excursion360_Builder.Shared.States.Items.Field
{
    public class FieldItem : MonoBehaviour
    {
        public FieldVertex[] vertices;

        public string title;

        public ContentType contentType;

        public Texture texture;
        public VideoClip videoClip;


#if UNITY_EDITOR
        public bool isOpened;
#endif
        public void Reset()
        {
            vertices = new FieldVertex[]
            {
                new FieldVertex{ index = 0 },
                new FieldVertex{ index = 1, Orientation = Quaternion.AngleAxis(45, Vector3.up) },
                new FieldVertex{ index = 2, Orientation = Quaternion.AngleAxis(45, Vector3.up) * Quaternion.AngleAxis(45, Vector3.right) },
                new FieldVertex{ index = 3, Orientation = Quaternion.AngleAxis(45, Vector3.right) }
            };

            texture = null;
        }

        public enum ContentType
        {
            Photo,
            Video
        }
    }
}
