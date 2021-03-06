/*
This file is part of the iText (R) project.
Copyright (c) 1998-2016 iText Group NV
Authors: Bruno Lowagie, Paulo Soares, et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation with the addition of the
following permission added to Section 15 as permitted in Section 7(a):
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
OF THIRD PARTY RIGHTS

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License
along with this program; if not, see http://www.gnu.org/licenses or write to
the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
Boston, MA, 02110-1301 USA, or download the license from the following URL:
http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions
of this program must display Appropriate Legal Notices, as required under
Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License,
a covered work must retain the producer line in every PDF that is created
or manipulated using iText.

You can be released from the requirements of the license by purchasing
a commercial license. Buying such a license is mandatory as soon as you
develop commercial activities involving the iText software without
disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP,
serving PDFs on the fly in a web application, shipping iText with a closed
source product.

For more information, please contact iText Software Corp. at this
address: sales@itextpdf.com
*/
using System.Collections.Generic;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Properties;
using iText.Layout.Renderer;

namespace iText.Layout.Margincollapse {
    /// <summary>
    /// Rules of the margins collapsing are taken from Mozilla Developer Network:
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Box_Model/Mastering_margin_collapsing
    /// </summary>
    public class MarginsCollapseHandler {
        private IRenderer renderer;

        private MarginsCollapseInfo collapseInfo;

        private MarginsCollapseInfo childMarginInfo;

        private MarginsCollapseInfo prevChildMarginInfo;

        private int firstNotEmptyKidIndex = 0;

        private int processedChildrenNum = 0;

        private IList<IRenderer> rendererChildren = new List<IRenderer>();

        public MarginsCollapseHandler(IRenderer renderer, MarginsCollapseInfo marginsCollapseInfo) {
            this.renderer = renderer;
            this.collapseInfo = marginsCollapseInfo != null ? marginsCollapseInfo : new MarginsCollapseInfo();
        }

        public virtual void ProcessFixedHeightAdjustment(float heightDelta) {
            collapseInfo.SetBufferSpaceOnTop(collapseInfo.GetBufferSpaceOnTop() + heightDelta);
            collapseInfo.SetBufferSpaceOnBottom(collapseInfo.GetBufferSpaceOnBottom() + heightDelta);
        }

        public virtual MarginsCollapseInfo StartChildMarginsHandling(IRenderer child, Rectangle layoutBox) {
            rendererChildren.Add(child);
            int childIndex = processedChildrenNum++;
            bool childIsBlockElement = IsBlockElement(child);
            PrepareBoxForLayoutAttempt(layoutBox, childIndex, childIsBlockElement);
            if (childIsBlockElement) {
                childMarginInfo = CreateMarginsInfoForBlockChild(childIndex);
            }
            return this.childMarginInfo;
        }

        private MarginsCollapseInfo CreateMarginsInfoForBlockChild(int childIndex) {
            bool ignoreChildTopMargin = false;
            // always assume that current child might be the last on this area
            bool ignoreChildBottomMargin = LastChildMarginAdjoinedToParent(renderer);
            if (childIndex == firstNotEmptyKidIndex) {
                ignoreChildTopMargin = FirstChildMarginAdjoinedToParent(renderer);
            }
            MarginsCollapse childCollapseBefore;
            if (childIndex == 0) {
                MarginsCollapse parentCollapseBefore = collapseInfo.GetCollapseBefore();
                childCollapseBefore = ignoreChildTopMargin ? parentCollapseBefore : new MarginsCollapse();
            }
            else {
                MarginsCollapse prevChildCollapseAfter = prevChildMarginInfo != null ? prevChildMarginInfo.GetOwnCollapseAfter
                    () : null;
                childCollapseBefore = prevChildCollapseAfter != null ? prevChildCollapseAfter : new MarginsCollapse();
            }
            MarginsCollapse parentCollapseAfter = collapseInfo.GetCollapseAfter().Clone();
            MarginsCollapse childCollapseAfter = ignoreChildBottomMargin ? parentCollapseAfter : new MarginsCollapse();
            MarginsCollapseInfo childMarginsInfo = new MarginsCollapseInfo(ignoreChildTopMargin, ignoreChildBottomMargin
                , childCollapseBefore, childCollapseAfter);
            if (ignoreChildTopMargin && childIndex == firstNotEmptyKidIndex) {
                childMarginsInfo.SetBufferSpaceOnTop(collapseInfo.GetBufferSpaceOnTop());
            }
            if (ignoreChildBottomMargin) {
                childMarginsInfo.SetBufferSpaceOnBottom(collapseInfo.GetBufferSpaceOnBottom());
            }
            return childMarginsInfo;
        }

        public virtual void EndChildMarginsHandling(Rectangle layoutBox) {
            int childIndex = processedChildrenNum - 1;
            if (childMarginInfo != null) {
                if (firstNotEmptyKidIndex == childIndex && childMarginInfo.IsSelfCollapsing()) {
                    firstNotEmptyKidIndex = childIndex + 1;
                }
                collapseInfo.SetSelfCollapsing(collapseInfo.IsSelfCollapsing() && childMarginInfo.IsSelfCollapsing());
            }
            else {
                collapseInfo.SetSelfCollapsing(false);
            }
            if (firstNotEmptyKidIndex == childIndex && FirstChildMarginAdjoinedToParent(renderer)) {
                if (!collapseInfo.IsSelfCollapsing()) {
                    GetRidOfCollapseArtifactsAtopOccupiedArea();
                    if (childMarginInfo != null) {
                        ProcessUsedChildBufferSpaceOnTop(layoutBox);
                    }
                }
            }
            if (prevChildMarginInfo != null) {
                FixPrevChildOccupiedArea(childIndex);
                UpdatePrevKidIfSelfCollapsedAndTopAdjoinedToParent(prevChildMarginInfo.GetOwnCollapseAfter());
            }
            prevChildMarginInfo = childMarginInfo;
            childMarginInfo = null;
        }

        public virtual void StartMarginsCollapse(Rectangle parentBBox) {
            collapseInfo.GetCollapseBefore().JoinMargin(GetModelTopMargin(renderer));
            collapseInfo.GetCollapseAfter().JoinMargin(GetModelBottomMargin(renderer));
            if (!FirstChildMarginAdjoinedToParent(renderer)) {
                float topIndent = collapseInfo.GetCollapseBefore().GetCollapsedMarginsSize();
                ApplyTopMargin(parentBBox, topIndent);
            }
            if (!LastChildMarginAdjoinedToParent(renderer)) {
                float bottomIndent = collapseInfo.GetCollapseAfter().GetCollapsedMarginsSize();
                ApplyBottomMargin(parentBBox, bottomIndent);
            }
            // ignore current margins for now
            IgnoreModelTopMargin(renderer);
            IgnoreModelBottomMargin(renderer);
        }

        public virtual void EndMarginsCollapse(Rectangle layoutBox) {
            if (prevChildMarginInfo != null) {
                UpdatePrevKidIfSelfCollapsedAndTopAdjoinedToParent(prevChildMarginInfo.GetCollapseAfter());
            }
            bool couldBeSelfCollapsing = iText.Layout.Margincollapse.MarginsCollapseHandler.MarginsCouldBeSelfCollapsing
                (renderer);
            if (FirstChildMarginAdjoinedToParent(renderer)) {
                if (collapseInfo.IsSelfCollapsing() && !couldBeSelfCollapsing) {
                    AddNotYetAppliedTopMargin(layoutBox);
                }
            }
            collapseInfo.SetSelfCollapsing(collapseInfo.IsSelfCollapsing() && couldBeSelfCollapsing);
            MarginsCollapse ownCollapseAfter;
            bool lastChildMarginJoinedToParent = prevChildMarginInfo != null && prevChildMarginInfo.IsIgnoreOwnMarginBottom
                ();
            if (lastChildMarginJoinedToParent) {
                ownCollapseAfter = prevChildMarginInfo.GetOwnCollapseAfter();
            }
            else {
                ownCollapseAfter = new MarginsCollapse();
            }
            ownCollapseAfter.JoinMargin(GetModelBottomMargin(renderer));
            collapseInfo.SetOwnCollapseAfter(ownCollapseAfter);
            if (collapseInfo.IsSelfCollapsing()) {
                if (prevChildMarginInfo != null) {
                    collapseInfo.SetCollapseAfter(prevChildMarginInfo.GetCollapseAfter());
                }
                else {
                    collapseInfo.GetCollapseAfter().JoinMargin(collapseInfo.GetCollapseBefore());
                    collapseInfo.GetOwnCollapseAfter().JoinMargin(collapseInfo.GetCollapseBefore());
                }
                if (!collapseInfo.IsIgnoreOwnMarginBottom() && !collapseInfo.IsIgnoreOwnMarginTop()) {
                    float collapsedMargins = collapseInfo.GetCollapseAfter().GetCollapsedMarginsSize();
                    OverrideModelBottomMargin(renderer, collapsedMargins);
                }
            }
            else {
                MarginsCollapse marginsCollapseBefore = collapseInfo.GetCollapseBefore();
                if (!collapseInfo.IsIgnoreOwnMarginTop()) {
                    float collapsedMargins = marginsCollapseBefore.GetCollapsedMarginsSize();
                    OverrideModelTopMargin(renderer, collapsedMargins);
                }
                if (lastChildMarginJoinedToParent) {
                    collapseInfo.SetCollapseAfter(prevChildMarginInfo.GetCollapseAfter());
                }
                if (!collapseInfo.IsIgnoreOwnMarginBottom()) {
                    float collapsedMargins = collapseInfo.GetCollapseAfter().GetCollapsedMarginsSize();
                    OverrideModelBottomMargin(renderer, collapsedMargins);
                }
            }
        }

        private void UpdatePrevKidIfSelfCollapsedAndTopAdjoinedToParent(MarginsCollapse collapseAfter) {
            if (prevChildMarginInfo.IsSelfCollapsing() && prevChildMarginInfo.IsIgnoreOwnMarginTop()) {
                collapseInfo.GetCollapseBefore().JoinMargin(collapseAfter);
            }
        }

        private void PrepareBoxForLayoutAttempt(Rectangle layoutBox, int childIndex, bool childIsBlockElement) {
            if (prevChildMarginInfo != null) {
                bool prevChildHasAppliedCollapseAfter = !prevChildMarginInfo.IsIgnoreOwnMarginBottom() && (!prevChildMarginInfo
                    .IsSelfCollapsing() || !prevChildMarginInfo.IsIgnoreOwnMarginTop());
                if (prevChildHasAppliedCollapseAfter) {
                    layoutBox.SetHeight(layoutBox.GetHeight() + prevChildMarginInfo.GetCollapseAfter().GetCollapsedMarginsSize
                        ());
                }
                bool prevChildCanApplyCollapseAfter = !prevChildMarginInfo.IsSelfCollapsing() || !prevChildMarginInfo.IsIgnoreOwnMarginTop
                    ();
                if (!childIsBlockElement && prevChildCanApplyCollapseAfter) {
                    float ownCollapsedMargins = prevChildMarginInfo.GetOwnCollapseAfter().GetCollapsedMarginsSize();
                    layoutBox.SetHeight(layoutBox.GetHeight() - ownCollapsedMargins);
                }
            }
            else {
                if (childIndex > firstNotEmptyKidIndex) {
                    if (LastChildMarginAdjoinedToParent(renderer)) {
                        // restore layout box after inline element
                        float bottomIndent = collapseInfo.GetCollapseAfter().GetCollapsedMarginsSize() - collapseInfo.GetUsedBufferSpaceOnBottom
                            ();
                        // used space shall be always less or equal to collapsedMarginAfter size
                        collapseInfo.SetBufferSpaceOnBottom(collapseInfo.GetBufferSpaceOnBottom() + collapseInfo.GetUsedBufferSpaceOnBottom
                            ());
                        collapseInfo.SetUsedBufferSpaceOnBottom(0);
                        layoutBox.SetY(layoutBox.GetY() - bottomIndent);
                        layoutBox.SetHeight(layoutBox.GetHeight() + bottomIndent);
                    }
                }
            }
            if (!childIsBlockElement) {
                if (childIndex == firstNotEmptyKidIndex && FirstChildMarginAdjoinedToParent(renderer)) {
                    float topIndent = collapseInfo.GetCollapseBefore().GetCollapsedMarginsSize();
                    ApplyTopMargin(layoutBox, topIndent);
                }
                if (LastChildMarginAdjoinedToParent(renderer)) {
                    float bottomIndent = collapseInfo.GetCollapseAfter().GetCollapsedMarginsSize();
                    ApplyBottomMargin(layoutBox, bottomIndent);
                }
            }
        }

        private void ApplyTopMargin(Rectangle box, float topIndent) {
            float bufferLeftoversOnTop = collapseInfo.GetBufferSpaceOnTop() - topIndent;
            float usedTopBuffer = bufferLeftoversOnTop > 0 ? topIndent : collapseInfo.GetBufferSpaceOnTop();
            collapseInfo.SetUsedBufferSpaceOnTop(usedTopBuffer);
            SubtractUsedTopBufferFromBottomBuffer(usedTopBuffer);
            if (bufferLeftoversOnTop >= 0) {
                collapseInfo.SetBufferSpaceOnTop(bufferLeftoversOnTop);
                box.MoveDown(topIndent);
            }
            else {
                box.MoveDown(collapseInfo.GetBufferSpaceOnTop());
                collapseInfo.SetBufferSpaceOnTop(0);
                box.SetHeight(box.GetHeight() + bufferLeftoversOnTop);
            }
        }

        private void ApplyBottomMargin(Rectangle box, float bottomIndent) {
            // Here we don't subtract used buffer space from topBuffer, because every kid is assumed to be 
            // the last one on the page, and so every kid always has parent's bottom buffer, however only the true last kid
            // uses it for real. Also, bottom margin are always applied after top margins, so it doesn't matter anyway.
            float bottomIndentLeftovers = bottomIndent - collapseInfo.GetBufferSpaceOnBottom();
            if (bottomIndentLeftovers < 0) {
                collapseInfo.SetUsedBufferSpaceOnBottom(bottomIndent);
                collapseInfo.SetBufferSpaceOnBottom(-bottomIndentLeftovers);
            }
            else {
                collapseInfo.SetUsedBufferSpaceOnBottom(collapseInfo.GetBufferSpaceOnBottom());
                collapseInfo.SetBufferSpaceOnBottom(0);
                box.SetY(box.GetY() + bottomIndentLeftovers);
                box.SetHeight(box.GetHeight() - bottomIndentLeftovers);
            }
        }

        private void ProcessUsedChildBufferSpaceOnTop(Rectangle layoutBox) {
            float childUsedBufferSpaceOnTop = childMarginInfo.GetUsedBufferSpaceOnTop();
            if (childUsedBufferSpaceOnTop > 0) {
                if (childUsedBufferSpaceOnTop > collapseInfo.GetBufferSpaceOnTop()) {
                    childUsedBufferSpaceOnTop = collapseInfo.GetBufferSpaceOnTop();
                }
                collapseInfo.SetBufferSpaceOnTop(collapseInfo.GetBufferSpaceOnTop() - childUsedBufferSpaceOnTop);
                collapseInfo.SetUsedBufferSpaceOnTop(childUsedBufferSpaceOnTop);
                // usage of top buffer space on child is expressed by moving layout box down instead of making it smaller,
                // so in order to process next kids correctly, we need to move parent layout box also
                layoutBox.MoveDown(childUsedBufferSpaceOnTop);
                SubtractUsedTopBufferFromBottomBuffer(childUsedBufferSpaceOnTop);
            }
        }

        private void SubtractUsedTopBufferFromBottomBuffer(float usedTopBuffer) {
            if (collapseInfo.GetBufferSpaceOnTop() > collapseInfo.GetBufferSpaceOnBottom()) {
                float bufferLeftoversOnTop = collapseInfo.GetBufferSpaceOnTop() - usedTopBuffer;
                if (bufferLeftoversOnTop < collapseInfo.GetBufferSpaceOnBottom()) {
                    collapseInfo.SetBufferSpaceOnBottom(bufferLeftoversOnTop);
                }
            }
            else {
                collapseInfo.SetBufferSpaceOnBottom(collapseInfo.GetBufferSpaceOnBottom() - usedTopBuffer);
            }
        }

        private void FixPrevChildOccupiedArea(int childIndex) {
            IRenderer prevRenderer = GetRendererChild(childIndex - 1);
            Rectangle bBox = prevRenderer.GetOccupiedArea().GetBBox();
            bool prevChildHasAppliedCollapseAfter = !prevChildMarginInfo.IsIgnoreOwnMarginBottom() && (!prevChildMarginInfo
                .IsSelfCollapsing() || !prevChildMarginInfo.IsIgnoreOwnMarginTop());
            if (prevChildHasAppliedCollapseAfter) {
                float bottomMargin = prevChildMarginInfo.GetCollapseAfter().GetCollapsedMarginsSize();
                bBox.SetHeight(bBox.GetHeight() - bottomMargin);
                bBox.MoveUp(bottomMargin);
                IgnoreModelBottomMargin(prevRenderer);
            }
            bool isNotBlockChild = !IsBlockElement(GetRendererChild(childIndex));
            bool prevChildCanApplyCollapseAfter = !prevChildMarginInfo.IsSelfCollapsing() || !prevChildMarginInfo.IsIgnoreOwnMarginTop
                ();
            if (isNotBlockChild && prevChildCanApplyCollapseAfter) {
                float ownCollapsedMargins = prevChildMarginInfo.GetOwnCollapseAfter().GetCollapsedMarginsSize();
                bBox.SetHeight(bBox.GetHeight() + ownCollapsedMargins);
                bBox.MoveDown(ownCollapsedMargins);
                OverrideModelBottomMargin(prevRenderer, ownCollapsedMargins);
            }
        }

        private void AddNotYetAppliedTopMargin(Rectangle layoutBox) {
            // normally, space for margins is added when content is met, however if all kids were self-collapsing (i.e. 
            // had no content) or if there were no kids, we need to add it when no more adjoining margins will be met
            float indentTop = collapseInfo.GetCollapseBefore().GetCollapsedMarginsSize();
            renderer.GetOccupiedArea().GetBBox().MoveDown(indentTop);
            // even though all kids have been already drawn, we still need to adjust layout box in case we are in the block of fixed size  
            ApplyTopMargin(layoutBox, indentTop);
        }

        private IRenderer GetRendererChild(int index) {
            return rendererChildren[index];
        }

        private void GetRidOfCollapseArtifactsAtopOccupiedArea() {
            Rectangle bBox = renderer.GetOccupiedArea().GetBBox();
            bBox.SetHeight(bBox.GetHeight() - collapseInfo.GetCollapseBefore().GetCollapsedMarginsSize());
        }

        private static bool MarginsCouldBeSelfCollapsing(IRenderer renderer) {
            return !(renderer is TableRenderer) && !HasBottomBorders(renderer) && !HasTopBorders(renderer) && !HasBottomPadding
                (renderer) && !HasTopPadding(renderer) && !HasPositiveHeight(renderer);
        }

        // table is never self-collapsing for now
        private static bool FirstChildMarginAdjoinedToParent(IRenderer parent) {
            return !(parent is RootRenderer) && !(parent is TableRenderer) && !HasTopBorders(parent) && !HasTopPadding
                (parent);
        }

        private static bool LastChildMarginAdjoinedToParent(IRenderer parent) {
            return !(parent is RootRenderer) && !(parent is TableRenderer) && !HasBottomBorders(parent) && !HasBottomPadding
                (parent) && !HasHeightProp(parent);
        }

        private static bool IsBlockElement(IRenderer renderer) {
            return renderer is BlockRenderer || renderer is TableRenderer;
        }

        private static bool HasHeightProp(IRenderer renderer) {
            // in mozilla and chrome height always prevents margins collapse in all cases.
            return renderer.GetModelElement().HasProperty(Property.HEIGHT);
        }

        // "min-height" property affects margins collapse differently in chrome and mozilla. While in chrome, this property
        // seems to not have any effect on collapsing margins at all (child margins collapse with parent margins even if
        // there is a considerable space between them due to the min-height property on parent), mozilla behaves better
        // and collapse happens only in case min-height of parent is less than actual height of the content and therefore
        // collapse really should happen. However even in mozilla, if parent has min-height which is a little bigger then
        // it's content actual height and margin collapse doesn't happen, in this case the child's margin is not shown fully however.
        //
        // || styles.containsKey(CssConstants.MIN_HEIGHT)
        // "max-height" doesn't seem to affect margins collapse in any way at least in chrome.
        // In mozilla it affects collapsing when parent's max-height is less than children actual height,
        // in this case collapse doesn't happen. However, at the moment in iText we won't show anything at all if
        // kid's height is bigger than parent's max-height, therefore this logic is irrelevant now anyway.
        //
        // || (includingMaxHeight && styles.containsKey(CssConstants.MAX_HEIGHT));
        private static bool HasPositiveHeight(IRenderer renderer) {
            return renderer.GetOccupiedArea().GetBBox().GetHeight() > 0;
        }

        private static bool HasTopPadding(IRenderer renderer) {
            float? padding = renderer.GetModelElement().GetProperty<float?>(Property.PADDING_TOP);
            return padding != null && padding > 0;
        }

        private static bool HasBottomPadding(IRenderer renderer) {
            float? padding = renderer.GetModelElement().GetProperty<float?>(Property.PADDING_TOP);
            return padding != null && padding > 0;
        }

        private static bool HasTopBorders(IRenderer renderer) {
            IPropertyContainer modelElement = renderer.GetModelElement();
            return modelElement.HasProperty(Property.BORDER_TOP) || modelElement.HasProperty(Property.BORDER);
        }

        private static bool HasBottomBorders(IRenderer renderer) {
            IPropertyContainer modelElement = renderer.GetModelElement();
            return modelElement.HasProperty(Property.BORDER_BOTTOM) || modelElement.HasProperty(Property.BORDER);
        }

        private static float GetModelTopMargin(IRenderer renderer) {
            float? margin = renderer.GetModelElement().GetProperty<float?>(Property.MARGIN_TOP);
            return margin != null ? (float)margin : 0;
        }

        private static void IgnoreModelTopMargin(IRenderer renderer) {
            renderer.SetProperty(Property.MARGIN_TOP, 0);
        }

        private static void OverrideModelTopMargin(IRenderer renderer, float collapsedMargins) {
            renderer.SetProperty(Property.MARGIN_TOP, collapsedMargins);
        }

        private static float GetModelBottomMargin(IRenderer renderer) {
            float? margin = renderer.GetModelElement().GetProperty<float?>(Property.MARGIN_BOTTOM);
            return margin != null ? (float)margin : 0;
        }

        private static void IgnoreModelBottomMargin(IRenderer renderer) {
            renderer.SetProperty(Property.MARGIN_BOTTOM, 0);
        }

        private static void OverrideModelBottomMargin(IRenderer renderer, float collapsedMargins) {
            renderer.SetProperty(Property.MARGIN_BOTTOM, collapsedMargins);
        }
    }
}
