using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleMaskCutoff;
using System;

namespace DiceRoller
{
    public class TileRange : MonoBehaviour
    {
        [Flags]
        public enum Adj
        { 
            None = 0,
            TopLeft = 1,
            Top = 2,
            TopRight = 4,
            Left = 8,
            Mid = 16,
            Right = 32,
            BottomLeft = 64,
            Bottom = 128,
            BottomRight = 256
        }

        [Header("Component")]
        public List<SpriteRenderer> frameRenderers = new List<SpriteRenderer>();
        public List<SpriteRenderer> backgroundRenderers = new List<SpriteRenderer>();

        // resources
        private TileStyle tileStyle = null;

        // ========================================================= Inquries =========================================================

        public bool IsShowing()
        {
            return frameRenderers[0].gameObject.activeSelf;
        }

        // ========================================================= Functionalities =========================================================

        public void SetTileStyle(TileStyle tileStyle)
        {
            this.tileStyle = tileStyle;
        }

        public void Show(bool show)
        {
            foreach (SpriteRenderer frameRenderer in frameRenderers)
            {
                frameRenderer.gameObject.SetActive(show);
            }
            foreach (SpriteRenderer backgroundRenderer in backgroundRenderers)
            {
                backgroundRenderer.gameObject.SetActive(show);
            }
        }

        public void SetColor(Color frameColor, Color backgroundColor)
        {
            foreach (SpriteRenderer frameRenderer in frameRenderers)
            {
                frameRenderer.color = frameColor;
            }
            foreach (SpriteRenderer backgroundRenderer in backgroundRenderers)
            {
                backgroundRenderer.color = backgroundColor;
            }
        }

        public void SetAdjancencies(Adj adjacencies, bool dashed)
        {
            /*
             * Note: 
             * 1) Renderers should be in the order of 0: TopLeft, 1: TopRight, 2: BottomLeft, 3: BottomRight
             * 2) Range index should be in the order of the followings
             * 
             * xxxxxxxx  xxxxxxxx  x                x
             * x                x                    
             * x   0         1  x      2         3   
             * x                x                    
             * x                x                    
             *
             * x                x                    
             * x                x                    
             * x   4         5  x      6         7   
             * x                x                    
             * xxxxxxxx  xxxxxxxx  x                x
             * 
             * x                x  xxxxxxxx  xxxxxxxx
             * x                x                    
             * x   8         9  x     10        11   
             * x                x                    
             * x                x                    
             * 
             * x                x                    
             * x                x                    
             * x  12        13  x     14        15   
             * x                x                    
             * x                x  xxxxxxxx  xxxxxxxx
             * 
             * ........  ........                    
             * ........  ........                    
             * ...16...  ...17...     18        19   
             * ........  ........                    
             * ........  ........                    
             * 
             * ........  ........                    
             * ........  ........                    
             * ...20...  ...21...     22        23   
             * ........  ........                    
             * ........  ........                    
             * 
             */

            int r0 =
                (adjacencies & (Adj.TopLeft | Adj.Top | Adj.Left)) == (Adj.None) ? 0 :
                (adjacencies & (Adj.TopLeft | Adj.Top | Adj.Left)) == (Adj.TopLeft) ? 0 :
                (adjacencies & (Adj.TopLeft | Adj.Top | Adj.Left)) == (Adj.Top) ? 8 :
                (adjacencies & (Adj.TopLeft | Adj.Top | Adj.Left)) == (Adj.Top | Adj.TopLeft) ? 8 :
                (adjacencies & (Adj.TopLeft | Adj.Top | Adj.Left)) == (Adj.Left) ? 10 :
                (adjacencies & (Adj.TopLeft | Adj.Top | Adj.Left)) == (Adj.Left | Adj.TopLeft) ? 10 :
                (adjacencies & (Adj.TopLeft | Adj.Top | Adj.Left)) == (Adj.Top | Adj.Left) ? 2 :
                (adjacencies & (Adj.TopLeft | Adj.Top | Adj.Left)) == (Adj.Top | Adj.Left | Adj.TopLeft) ? 16 :
                18;

            int r1 =
                (adjacencies & (Adj.TopRight | Adj.Top | Adj.Right)) == (Adj.None) ? 1 :
                (adjacencies & (Adj.TopRight | Adj.Top | Adj.Right)) == (Adj.TopRight) ? 1 :
                (adjacencies & (Adj.TopRight | Adj.Top | Adj.Right)) == (Adj.Top) ? 9 :
                (adjacencies & (Adj.TopRight | Adj.Top | Adj.Right)) == (Adj.Top | Adj.TopRight) ? 9 :
                (adjacencies & (Adj.TopRight | Adj.Top | Adj.Right)) == (Adj.Right) ? 11 :
                (adjacencies & (Adj.TopRight | Adj.Top | Adj.Right)) == (Adj.Right | Adj.TopRight) ? 11 :
                (adjacencies & (Adj.TopRight | Adj.Top | Adj.Right)) == (Adj.Top | Adj.Right) ? 3:
                (adjacencies & (Adj.TopRight | Adj.Top | Adj.Right)) == (Adj.Top | Adj.Right | Adj.TopRight) ? 17 :
                19;

            int r2 =
                (adjacencies & (Adj.BottomLeft | Adj.Bottom | Adj.Left)) == (Adj.None) ? 4 :
                (adjacencies & (Adj.BottomLeft | Adj.Bottom | Adj.Left)) == (Adj.BottomLeft) ? 4 :
                (adjacencies & (Adj.BottomLeft | Adj.Bottom | Adj.Left)) == (Adj.Bottom) ? 12 :
                (adjacencies & (Adj.BottomLeft | Adj.Bottom | Adj.Left)) == (Adj.Bottom | Adj.BottomLeft) ? 12 :
                (adjacencies & (Adj.BottomLeft | Adj.Bottom | Adj.Left)) == (Adj.Left) ? 14 :
                (adjacencies & (Adj.BottomLeft | Adj.Bottom | Adj.Left)) == (Adj.Left | Adj.BottomLeft) ? 14 :
                (adjacencies & (Adj.BottomLeft | Adj.Bottom | Adj.Left)) == (Adj.Bottom | Adj.Left) ? 6 :
                (adjacencies & (Adj.BottomLeft | Adj.Bottom | Adj.Left)) == (Adj.Bottom | Adj.Left | Adj.BottomLeft) ? 20 :
                19;

            int r3 =
                (adjacencies & (Adj.BottomRight | Adj.Bottom | Adj.Right)) == (Adj.None) ? 5 :
                (adjacencies & (Adj.BottomRight | Adj.Bottom | Adj.Right)) == (Adj.BottomRight) ? 5 :
                (adjacencies & (Adj.BottomRight | Adj.Bottom | Adj.Right)) == (Adj.Bottom) ? 13 :
                (adjacencies & (Adj.BottomRight | Adj.Bottom | Adj.Right)) == (Adj.Bottom | Adj.BottomRight) ? 13 :
                (adjacencies & (Adj.BottomRight | Adj.Bottom | Adj.Right)) == (Adj.Right) ? 15 :
                (adjacencies & (Adj.BottomRight | Adj.Bottom | Adj.Right)) == (Adj.Right | Adj.BottomRight) ? 15 :
                (adjacencies & (Adj.BottomRight | Adj.Bottom | Adj.Right)) == (Adj.Bottom | Adj.Right) ? 7 :
                (adjacencies & (Adj.BottomRight | Adj.Bottom | Adj.Right)) == (Adj.Bottom | Adj.Right | Adj.BottomRight) ? 21 :
                20;

            if (dashed)
            {
                frameRenderers[0].sprite = tileStyle.frameDashedSprites[r0];
                frameRenderers[1].sprite = tileStyle.frameDashedSprites[r1];
                frameRenderers[2].sprite = tileStyle.frameDashedSprites[r2];
                frameRenderers[3].sprite = tileStyle.frameDashedSprites[r3];
            }
            else
            {
                frameRenderers[0].sprite = tileStyle.frameSolidSprites[r0];
                frameRenderers[1].sprite = tileStyle.frameSolidSprites[r1];
                frameRenderers[2].sprite = tileStyle.frameSolidSprites[r2];
                frameRenderers[3].sprite = tileStyle.frameSolidSprites[r3];
            }

            backgroundRenderers[0].sprite = tileStyle.frameMasks[r0];
            backgroundRenderers[1].sprite = tileStyle.frameMasks[r1];
            backgroundRenderers[2].sprite = tileStyle.frameMasks[r2];
            backgroundRenderers[3].sprite = tileStyle.frameMasks[r3];
        }

        public void SetSpriteOrder(int order)
        {
            foreach (SpriteRenderer sr in frameRenderers)
            {
                sr.sortingOrder = order * 2 + 1;
            }
            foreach (SpriteRenderer sr in backgroundRenderers)
            {
                sr.sortingOrder = order * 2;
            }
        }
    }
}
