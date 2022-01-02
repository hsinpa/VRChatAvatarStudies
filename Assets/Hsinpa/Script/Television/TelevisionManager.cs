using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.TV
{
    public class TelevisionManager : MonoBehaviour
    {
        [SerializeField]
        private TelevisionView televisionViewPrefab;

        [SerializeField]
        private int row;

        [SerializeField]
        private int column;

        [SerializeField]
        private float space;

        [Header("Debug")]
        [SerializeField]
        private Texture debugTexture;

        private Vector2 totalSize
        {
            get
            {
                Vector2 t = new Vector2();
                t.x = row * (televisionViewPrefab.size.x + space );
                t.y = column * (televisionViewPrefab.size.y + space);
                return t;
            }
        }

        private TelevisionView[] televisionViewArray;

        public void GenerateTelevisionSet()
        {
            Utility.UtilityFunc.ClearChildObject(televisionViewArray);

            Vector2 _totalSize = totalSize;
            float xoffset = (televisionViewPrefab.size.x ) * 0.5f;
            float yoffset = (televisionViewPrefab.size.x ) * 0.5f;
            Vector3 topLeftCorner = new Vector3(transform.position.x - (_totalSize.x * 0.5f) + xoffset, 
                                                transform.position.y + (_totalSize.y*0.5f) - yoffset, 
                                                transform.position.z);
            Vector3 boundSize = televisionViewPrefab.size;

            televisionViewArray = new TelevisionView[row * row];

            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < row; y++)
                {
                    int index = (y * row) + x;

                    Vector3 spawnPosition = GetPositionByIndex(topLeftCorner, boundSize, x, y);

                    TelevisionView tvView = CreateSingleTVView(spawnPosition, index);
                    televisionViewArray[index] = tvView;
                }
            }
        }

        private TelevisionView CreateSingleTVView(Vector3 position, int index) {
            TelevisionView generateTVView = GameObject.Instantiate<TelevisionView>(televisionViewPrefab, this.transform);

            generateTVView.transform.position = position;

            return generateTVView;
        }

        private Vector3 GetPositionByIndex(Vector3 topLeftCorner, Vector3 boundSize, int x, int y)
        {
            Vector3 position = new Vector3();

            position.x = topLeftCorner.x + (x * (boundSize.x + space ));
            position.y = topLeftCorner.y - (y * (boundSize.y + space));
            position.z = topLeftCorner.z;


            int xRemainig = row % 2;
            int yRemainig = column % 2;
            float xOffset = (xRemainig == 0) ? space * 0.5f : space * 0.5f;
            float yOffset = (yRemainig == 0) ? -space * 0.5f : -space * 0.5f;

            position.x += xOffset;
            position.y += yOffset;

            return position;
        }

    }
}