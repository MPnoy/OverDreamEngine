#region MIT License
/*Copyright (c) 2012-2016 Robert Rouhani <robert.rouhani@gmail.com>

SharpFont based on Tao.FreeType, Copyright (c) 2003-2007 Tao Framework Team

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/
#endregion

using System;
using System.Runtime.InteropServices;
using SharpFont.TrueType;
using System.Reflection;
using FTSharp = FreeTypeSharp.Native.FT;
using FreeTypeSharp.Native;

namespace SharpFont
{
    /// <content>
    /// This file contains all the raw FreeType2 function signatures.
    /// </content>
    public static partial class FT
    {
        static bool? isMacOS;

        /// <summary>
        /// Returns true if the current .net platform is macOS.
        /// </summary>
        internal static bool IsMacOS
        {
            get
            {
                if (isMacOS != null)
                {
                    return isMacOS.Value;
                }
                else
                {
                    lock (typeof(FT))
                    {
                        if (isMacOS == null) // repeat the test
                        {
                            isMacOS = false;

                            var os = typeof(Environment)
                                ?.GetRuntimeProperty("OSVersion")
                                ?.GetValue(null);

                            var platformObj = os
                                ?.GetType().GetRuntimeProperty("Platform")
                                ?.GetValue(os);

                            if (platformObj != null)
                            {
                                var platform = (int)platformObj;
                                if (platform == 6)
                                {
                                    isMacOS = true;
                                }
                            }
                        }
                    }
                }

                return isMacOS.Value;
            }
        }

        #region Base Interface

        internal static Error FT_Init_FreeType(out IntPtr alibrary) =>
            (Error)FTSharp.FT_Init_FreeType(out alibrary);

        internal static Error FT_Done_FreeType(IntPtr library) =>
            (Error)FTSharp.FT_Done_FreeType(library);

        internal static Error FT_New_Face(IntPtr library, string filepathname, int face_index, out IntPtr aface) =>
            (Error)FTSharp.FT_New_Face(library, filepathname, face_index, out aface);

        internal static Error FT_New_Memory_Face(IntPtr library, IntPtr file_base, int file_size, int face_index, out IntPtr aface) =>
            (Error)FTSharp.FT_New_Memory_Face(library, file_base, file_size, face_index, out aface);

        internal static Error FT_Open_Face(IntPtr library, IntPtr args, int face_index, out IntPtr aface) =>
            (Error)FTSharp.FT_Open_Face(library, args, face_index, out aface);

        internal static Error FT_Attach_File(IntPtr face, string filepathname) =>
            (Error)FTSharp.FT_Attach_File(face, filepathname);

        internal static Error FT_Attach_Stream(IntPtr face, IntPtr parameters) =>
            (Error)FTSharp.FT_Attach_Stream(face, parameters);

        internal static Error FT_Reference_Face(IntPtr face) =>
            (Error)FTSharp.FT_Reference_Face(face);

        internal static Error FT_Done_Face(IntPtr face) =>
            (Error)FTSharp.FT_Done_Face(face);

        internal static Error FT_Select_Size(IntPtr face, int strike_index) =>
            (Error)FTSharp.FT_Select_Size(face, strike_index);

        internal static Error FT_Request_Size(IntPtr face, IntPtr req) =>
            (Error)FTSharp.FT_Request_Size(face, req);

        internal static Error FT_Set_Char_Size(IntPtr face, IntPtr char_width, IntPtr char_height, uint horz_resolution, uint vert_resolution) =>
            (Error)FTSharp.FT_Set_Char_Size(face, char_width, char_height, horz_resolution, vert_resolution);

        internal static Error FT_Set_Pixel_Sizes(IntPtr face, uint pixel_width, uint pixel_height) =>
            (Error)FTSharp.FT_Set_Pixel_Sizes(face, pixel_width, pixel_height);

        internal static Error FT_Load_Glyph(IntPtr face, uint glyph_index, int load_flags) =>
            (Error)FTSharp.FT_Load_Glyph(face, glyph_index, load_flags);

        internal static Error FT_Load_Char(IntPtr face, uint char_code, int load_flags) =>
            (Error)FTSharp.FT_Load_Char(face, char_code, load_flags);

        internal static void FT_Set_Transform(IntPtr face, IntPtr matrix, IntPtr delta) =>
            FTSharp.FT_Set_Transform(face, matrix, delta);

        internal static Error FT_Render_Glyph(IntPtr slot, RenderMode render_mode) =>
            (Error)FTSharp.FT_Render_Glyph(slot, (FreeTypeSharp.Native.FT_Render_Mode)render_mode);

        internal static Error FT_Get_Kerning(IntPtr face, uint left_glyph, uint right_glyph, uint kern_mode, out FTVector26Dot6 akerning)
        {
            var ret = (Error)FTSharp.FT_Get_Kerning(face, left_glyph, right_glyph, kern_mode, out var akerning1);
            akerning = akerning1;
            return ret;
        }

        internal static Error FT_Get_Track_Kerning(IntPtr face, IntPtr point_size, int degree, out IntPtr akerning) =>
            (Error)FTSharp.FT_Get_Track_Kerning(face, point_size, degree, out akerning);

        internal static Error FT_Get_Glyph_Name(IntPtr face, uint glyph_index, IntPtr buffer, uint buffer_max) =>
            (Error)FTSharp.FT_Get_Glyph_Name(face, glyph_index, buffer, buffer_max);

        internal static IntPtr FT_Get_Postscript_Name(IntPtr face) =>
            (IntPtr)FTSharp.FT_Get_Postscript_Name(face);

        internal static Error FT_Select_Charmap(IntPtr face, Encoding encoding) =>
            (Error)FTSharp.FT_Select_Charmap(face, (FreeTypeSharp.Native.FT_Encoding)encoding);

        internal static Error FT_Set_Charmap(IntPtr face, IntPtr charmap) =>
            (Error)FTSharp.FT_Set_Charmap(face, charmap);

        internal static int FT_Get_Charmap_Index(IntPtr charmap) =>
            (int)FTSharp.FT_Get_Charmap_Index(charmap);

        internal static uint FT_Get_Char_Index(IntPtr face, uint charcode) =>
            (uint)FTSharp.FT_Get_Char_Index(face, charcode);

        internal static uint FT_Get_First_Char(IntPtr face, out uint agindex) =>
            (uint)FTSharp.FT_Get_First_Char(face, out agindex);

        internal static uint FT_Get_Next_Char(IntPtr face, uint char_code, out uint agindex) =>
            (uint)FTSharp.FT_Get_Next_Char(face, char_code, out agindex);

        internal static uint FT_Get_Name_Index(IntPtr face, IntPtr glyph_name) =>
            (uint)FTSharp.FT_Get_Name_Index(face, glyph_name);

        internal static Error FT_Get_SubGlyph_Info(IntPtr glyph, uint sub_index, out int p_index, out SubGlyphFlags p_flags, out int p_arg1, out int p_arg2, out FTMatrix p_transform)
        {
            var ret = (Error)FTSharp.FT_Get_SubGlyph_Info(glyph, sub_index, out p_index, out var p_flags1, out p_arg1, out p_arg2, out var p_transform1);
            p_flags = (SubGlyphFlags)p_flags1;
            p_transform = p_transform1;
            return ret;
        }

        internal static EmbeddingTypes FT_Get_FSType_Flags(IntPtr face) =>
            (EmbeddingTypes)FTSharp.FT_Get_FSType_Flags(face);

        #endregion

        #region Glyph Variants

        internal static uint FT_Face_GetCharVariantIndex(IntPtr face, uint charcode, uint variantSelector) =>
            (uint)FTSharp.FT_Face_GetCharVariantIndex(face, charcode, variantSelector);

        internal static int FT_Face_GetCharVariantIsDefault(IntPtr face, uint charcode, uint variantSelector) =>
            (int)FTSharp.FT_Face_GetCharVariantIsDefault(face, charcode, variantSelector);

        internal static IntPtr FT_Face_GetVariantSelectors(IntPtr face) =>
            (IntPtr)FTSharp.FT_Face_GetVariantSelectors(face);

        internal static IntPtr FT_Face_GetVariantsOfChar(IntPtr face, uint charcode) =>
            (IntPtr)FTSharp.FT_Face_GetVariantsOfChar(face, charcode);

        internal static IntPtr FT_Face_GetCharsOfVariant(IntPtr face, uint variantSelector) =>
            (IntPtr)FTSharp.FT_Face_GetCharsOfVariant(face, variantSelector);

        #endregion

        #region Glyph Management

        internal static Error FT_Get_Glyph(IntPtr slot, out IntPtr aglyph) =>
            (Error)FTSharp.FT_Get_Glyph(slot, out aglyph);

        internal static Error FT_Glyph_Copy(IntPtr source, out IntPtr target) =>
            (Error)FTSharp.FT_Glyph_Copy(source, out target);

        internal static Error FT_Glyph_Transform(IntPtr glyph, ref FTMatrix matrix, ref FTVector delta)
        {
            FT_Matrix matrix1 = matrix;
            FT_Vector delta1 = delta;
            var ret = (Error)FTSharp.FT_Glyph_Transform(glyph, ref matrix1, ref delta1);
            matrix = matrix1;
            delta = delta1;
            return ret;
        }

        internal static void FT_Glyph_Get_CBox(IntPtr glyph, GlyphBBoxMode bbox_mode, out BBox acbox)
        {
            FTSharp.FT_Glyph_Get_CBox(glyph, (FT_Glyph_BBox_Mode)bbox_mode, out var acbox1);
            acbox = acbox1;
        }

        internal static Error FT_Glyph_To_Bitmap(ref IntPtr the_glyph, RenderMode render_mode, ref FTVector26Dot6 origin, [MarshalAs(UnmanagedType.U1)] bool destroy)
        {
            FT_Vector origin1 = origin;
            var ret = (Error)FTSharp.FT_Glyph_To_Bitmap(ref the_glyph, (FT_Render_Mode)render_mode, ref origin1, destroy);
            origin = origin1;
            return ret;
        }

        internal static void FT_Done_Glyph(IntPtr glyph) =>
            FTSharp.FT_Done_Glyph(glyph);

        #endregion

        #region Mac Specific Interface - check for macOS before calling these methods.

        internal static Error FT_New_Face_From_FOND(IntPtr library, IntPtr fond, int face_index, out IntPtr aface) =>
            (Error)FTSharp.FT_New_Face_From_FOND(library, fond, face_index, out aface);

        internal static Error FT_GetFile_From_Mac_Name(string fontName, out IntPtr pathSpec, out int face_index) =>
            (Error)FTSharp.FT_GetFile_From_Mac_Name(fontName, out pathSpec, out face_index);

        internal static Error FT_GetFile_From_Mac_ATS_Name(string fontName, out IntPtr pathSpec, out int face_index) =>
            (Error)FTSharp.FT_GetFile_From_Mac_ATS_Name(fontName, out pathSpec, out face_index);

        internal static Error FT_GetFilePath_From_Mac_ATS_Name(string fontName, IntPtr path, int maxPathSize, out int face_index) =>
            (Error)FTSharp.FT_GetFilePath_From_Mac_ATS_Name(fontName, path, maxPathSize, out face_index);

        internal static Error FT_New_Face_From_FSSpec(IntPtr library, IntPtr spec, int face_index, out IntPtr aface) =>
            (Error)FTSharp.FT_New_Face_From_FSSpec(library, spec, face_index, out aface);

        internal static Error FT_New_Face_From_FSRef(IntPtr library, IntPtr @ref, int face_index, out IntPtr aface) =>
            (Error)FTSharp.FT_New_Face_From_FSRef(library, @ref, face_index, out aface);
        #endregion

        #region Size Management

        internal static Error FT_New_Size(IntPtr face, out IntPtr size) =>
            (Error)FTSharp.FT_New_Size(face, out size);

        internal static Error FT_Done_Size(IntPtr size) =>
            (Error)FTSharp.FT_Done_Size(size);

        internal static Error FT_Activate_Size(IntPtr size) =>
            (Error)FTSharp.FT_Activate_Size(size);

        #endregion

        #region Format-Specific API

        #region Multiple Masters

        //internal static Error FT_Get_Multi_Master(IntPtr face, out IntPtr amaster) =>
        //    (Error)FTSharp.FT_Get_Multi_Master(face, out amaster);

        //internal static Error FT_Get_MM_Var(IntPtr face, out IntPtr amaster) =>
        //    (Error)FTSharp.FT_Get_MM_Var(face, out amaster);

        //internal static Error FT_Set_MM_Design_Coordinates(IntPtr face, uint num_coords, IntPtr coords) =>
        //    (Error)FTSharp.FT_Set_MM_Design_Coordinates(face, num_coords, coords);

        //internal static Error FT_Set_Var_Design_Coordinates(IntPtr face, uint num_coords, IntPtr coords) =>
        //    (Error)FTSharp.FT_Set_Var_Design_Coordinates(face, num_coords, coords);

        //internal static Error FT_Set_MM_Blend_Coordinates(IntPtr face, uint num_coords, IntPtr coords) =>
        //    (Error)FTSharp.FT_Set_MM_Blend_Coordinates(face, num_coords, coords);

        //internal static Error FT_Set_Var_Blend_Coordinates(IntPtr face, uint num_coords, IntPtr coords) =>
        //    (Error)FTSharp.FT_Set_Var_Blend_Coordinates(face, num_coords, coords);

        #endregion

        #region TrueType Tables

        //internal static IntPtr FT_Get_Sfnt_Table(IntPtr face, SfntTag tag) =>
        //    (IntPtr)FTSharp.FT_Get_Sfnt_Table(face, tag);

        ////TODO find FT_TRUETYPE_TAGS_H and create an enum for "tag"
        //internal static Error FT_Load_Sfnt_Table(IntPtr face, uint tag, int offset, IntPtr buffer, ref uint length) =>
        //    (Error)FTSharp.FT_Load_Sfnt_Table(face, uint tag, int offset, IntPtr buffer, ref uint length);

        //internal static 
        //Error FT_Sfnt_Table_Info(IntPtr face, uint table_index, SfntTag* tag, out uint length) =>
        //    (Error)FTSharp.FT_Sfnt_Table_Info(face,  table_index,  tag, out  length);

        //internal static uint FT_Get_CMap_Language_ID(IntPtr charmap) =>
        //    (uint)FTSharp.FT_Get_CMap_Language_ID(charmap);

        //internal static int FT_Get_CMap_Format(IntPtr charmap) =>
        //    (int)FTSharp.FT_Get_CMap_Format(charmap);

        #endregion

        #region Type 1 Tables

        //[return: MarshalAs(UnmanagedType.U1)]
        //internal static bool FT_Has_PS_Glyph_Names(IntPtr face) =>
        //    (bool)FTSharp.FT_Has_PS_Glyph_Names(face);

        //internal static Error FT_Get_PS_Font_Info(IntPtr face, out PostScript.Internal.FontInfoRec afont_info) =>
        //    (Error)FTSharp.FT_Get_PS_Font_Info(face);

        //internal static Error FT_Get_PS_Font_Private(IntPtr face, out PostScript.Internal.PrivateRec afont_private) =>
        //    (Error)FTSharp.FT_Get_PS_Font_Private(face);

        //internal static int FT_Get_PS_Font_Value(IntPtr face, DictionaryKeys key, uint idx, ref IntPtr value, int value_len) =>
        //    (int)FTSharp.FT_Get_PS_Font_Value(face);

        #endregion

        #region SFNT Names

        //internal static uint FT_Get_Sfnt_Name_Count(IntPtr face) =>
        //    (uint)FTSharp.FT_Get_Sfnt_Name_Count(face);

        //internal static Error FT_Get_Sfnt_Name(IntPtr face, uint idx, out TrueType.Internal.SfntNameRec aname) =>
        //    (Error)FTSharp.FT_Get_Sfnt_Name(face);

        #endregion

        #region BDF and PCF Files

        //internal static Error FT_Get_BDF_Charset_ID(IntPtr face, out string acharset_encoding, out string acharset_registry) =>
        //    (Error)FTSharp.FT_Get_BDF_Charset_ID(face);

        //internal static Error FT_Get_BDF_Property(IntPtr face, string prop_name, out IntPtr aproperty) =>
        //    (Error)FTSharp.FT_Get_BDF_Property(face);

        #endregion

        #region CID Fonts

        //internal static Error FT_Get_CID_Registry_Ordering_Supplement(IntPtr face, out string registry, out string ordering, out int aproperty) =>
        //    (Error)FTSharp.FT_Get_CID_Registry_Ordering_Supplement(face);

        //internal static Error FT_Get_CID_Is_Internally_CID_Keyed(IntPtr face, out byte is_cid) =>
        //    (Error)FTSharp.FT_Get_CID_Is_Internally_CID_Keyed(face);

        //internal static Error FT_Get_CID_From_Glyph_Index(IntPtr face, uint glyph_index, out uint cid) =>
        //    (Error)FTSharp.FT_Get_CID_From_Glyph_Index(face);

        #endregion

        #region PFR Fonts

        //internal static Error FT_Get_PFR_Metrics(IntPtr face, out uint aoutline_resolution, out uint ametrics_resolution, out IntPtr ametrics_x_scale, out IntPtr ametrics_y_scale) =>
        //    (Error)FTSharp.FT_Get_PFR_Metrics(face);

        //internal static Error FT_Get_PFR_Kerning(IntPtr face, uint left, uint right, out FTVector avector) =>
        //    (Error)FTSharp.FT_Get_PFR_Kerning(face);

        //internal static Error FT_Get_PFR_Advance(IntPtr face, uint gindex, out int aadvance) =>
        //    (Error)FTSharp.FT_Get_PFR_Advance(face);

        #endregion

        #region Window FNT Files

        //internal static Error FT_Get_WinFNT_Header(IntPtr face, out IntPtr aheader) =>
        //    (Error)FTSharp.FT_Get_WinFNT_Header(face, out aheader);

        #endregion

        #region Font Formats

        //internal static IntPtr FT_Get_X11_Font_Format(IntPtr face) =>
        //    (IntPtr)FTSharp.FT_Get_X11_Font_Format(face);

        #endregion

        #region Gasp Table

        //internal static Gasp FT_Get_Gasp(IntPtr face, uint ppem) =>
        //    (Gasp)FTSharp.FT_Get_Gasp(face);

        #endregion

        #endregion

        #region Support API

        #region Computations

        internal static IntPtr FT_MulDiv(IntPtr a, IntPtr b, IntPtr c) =>
            (IntPtr)FTSharp.FT_MulDiv(a, b, c);

        internal static IntPtr FT_MulFix(IntPtr a, IntPtr b) =>
            (IntPtr)FTSharp.FT_MulFix(a, b);

        internal static IntPtr FT_DivFix(IntPtr a, IntPtr b) =>
            (IntPtr)FTSharp.FT_DivFix(a, b);

        internal static IntPtr FT_RoundFix(IntPtr a) =>
            (IntPtr)FTSharp.FT_RoundFix(a);

        internal static IntPtr FT_CeilFix(IntPtr a) =>
            (IntPtr)FTSharp.FT_CeilFix(a);

        internal static IntPtr FT_FloorFix(IntPtr a) =>
            (IntPtr)FTSharp.FT_FloorFix(a);

        internal static void FT_Vector_Transform(ref FTVector vec, ref FTMatrix matrix)
        {
            FT_Vector vec1 = vec;
            FT_Matrix matrix1 = matrix;
            FTSharp.FT_Vector_Transform(ref vec1, ref matrix1);
            vec = vec1;
            matrix = matrix1;
        }

        internal static void FT_Matrix_Multiply(ref FTMatrix a, ref FTMatrix b)
        {
            FT_Matrix a1 = a;
            FT_Matrix b1 = b;
            FTSharp.FT_Matrix_Multiply(ref a1, ref b1);
            a = a1;
            b = b1;
        }

        internal static Error FT_Matrix_Invert(ref FTMatrix matrix)
        {
            FT_Matrix matrix1 = matrix;
            var ret = (Error)FTSharp.FT_Matrix_Invert(ref matrix1);
            matrix = matrix1;
            return ret;
        }

        internal static IntPtr FT_Sin(IntPtr angle) =>
            (IntPtr)FTSharp.FT_Sin(angle);

        internal static IntPtr FT_Cos(IntPtr angle) =>
            (IntPtr)FTSharp.FT_Cos(angle);

        internal static IntPtr FT_Tan(IntPtr angle) =>
            (IntPtr)FTSharp.FT_Tan(angle);

        internal static IntPtr FT_Atan2(IntPtr x, IntPtr y) =>
            (IntPtr)FTSharp.FT_Atan2(x, y);

        internal static IntPtr FT_Angle_Diff(IntPtr angle1, IntPtr angle2) =>
            (IntPtr)FTSharp.FT_Angle_Diff(angle1, angle2);

        internal static void FT_Vector_Unit(out FTVector vec, IntPtr angle)
        {
            FTSharp.FT_Vector_Unit(out var vec1, angle);
            vec = vec1;
        }

        internal static void FT_Vector_Rotate(ref FTVector vec, IntPtr angle)
        {
            FT_Vector vec1 = vec;
            FTSharp.FT_Vector_Rotate(ref vec1, angle);
            vec = vec1;
        }

        internal static IntPtr FT_Vector_Length(ref FTVector vec)
        {
            FT_Vector vec1 = vec;
            var ret = (IntPtr)FTSharp.FT_Vector_Length(ref vec1);
            vec = vec1;
            return ret;
        }

        internal static void FT_Vector_Polarize(ref FTVector vec, out IntPtr length, out IntPtr angle)
        {
            FT_Vector vec1 = vec;
            FTSharp.FT_Vector_Polarize(ref vec1, out length, out angle);
            vec = vec1;
        }

        internal static void FT_Vector_From_Polar(out FTVector vec, IntPtr length, IntPtr angle)
        {
            FTSharp.FT_Vector_From_Polar(out var vec1, length, angle);
            vec = vec1;
        }

        #endregion

        #region List Processing

        //internal static IntPtr FT_List_Find(IntPtr list, IntPtr data) =>
        //    (IntPtr)FTSharp.FT_List_Find(list);

        //internal static void FT_List_Add(IntPtr list, IntPtr node) =>
        //    (void)FTSharp.FT_List_Add(list);

        //internal static void FT_List_Insert(IntPtr list, IntPtr node) =>
        //    (void)FTSharp.FT_List_Insert(list);

        //internal static void FT_List_Remove(IntPtr list, IntPtr node) =>
        //    (void)FTSharp.FT_List_Remove(list);

        //internal static void FT_List_Up(IntPtr list, IntPtr node) =>
        //    (void)FTSharp.FT_List_Up(list);

        //internal static Error FT_List_Iterate(IntPtr list, ListIterator iterator, IntPtr user) =>
        //    (Error)FTSharp.FT_List_Iterate(list);

        //internal static void FT_List_Finalize(IntPtr list, ListDestructor destroy, IntPtr memory, IntPtr user) =>
        //    (void)FTSharp.FT_List_Finalize(list);

        #endregion

        #region Outline Processing

        //internal static Error FT_Outline_New(IntPtr library, uint numPoints, int numContours, out IntPtr anoutline) =>
        //    (Error)FTSharp.FT_Outline_New(library);

        //internal static Error FT_Outline_New_Internal(IntPtr memory, uint numPoints, int numContours, out IntPtr anoutline) =>
        //    (Error)FTSharp.FT_Outline_New_Internal(memory);

        //internal static Error FT_Outline_Done(IntPtr library, IntPtr outline) =>
        //    (Error)FTSharp.FT_Outline_Done(library);

        //internal static Error FT_Outline_Done_Internal(IntPtr memory, IntPtr outline) =>
        //    (Error)FTSharp.FT_Outline_Done_Internal(memory);

        //internal static Error FT_Outline_Copy(IntPtr source, ref IntPtr target) =>
        //    (Error)FTSharp.FT_Outline_Copy(source);

        //internal static void FT_Outline_Translate(IntPtr outline, int xOffset, int yOffset) =>
        //    (void)FTSharp.FT_Outline_Translate(outline);

        //internal static void FT_Outline_Transform(IntPtr outline, ref FTMatrix matrix) =>
        //    (void)FTSharp.FT_Outline_Transform(outline);

        //internal static Error FT_Outline_Embolden(IntPtr outline, IntPtr strength) =>
        //    (Error)FTSharp.FT_Outline_Embolden(outline);

        //internal static Error FT_Outline_EmboldenXY(IntPtr outline, int xstrength, int ystrength) =>
        //    (Error)FTSharp.FT_Outline_EmboldenXY(outline);

        //internal static void FT_Outline_Reverse(IntPtr outline) =>
        //    (void)FTSharp.FT_Outline_Reverse(outline);

        //internal static Error FT_Outline_Check(IntPtr outline) =>
        //    (Error)FTSharp.FT_Outline_Check(outline);

        //internal static Error FT_Outline_Get_BBox(IntPtr outline, out BBox abbox) =>
        //    (Error)FTSharp.FT_Outline_Get_BBox(outline, out abbox);

        //internal static Error FT_Outline_Decompose(IntPtr outline, ref OutlineFuncsRec func_interface, IntPtr user) =>
        //    (Error)FTSharp.FT_Outline_Decompose(outline, ref func_interface, user);

        //internal static void FT_Outline_Get_CBox(IntPtr outline, out BBox acbox) =>
        //    (void)FTSharp.FT_Outline_Get_CBox(outline, out acbox);

        internal static Error FT_Outline_Get_Bitmap(IntPtr library, IntPtr outline, IntPtr abitmap) =>
            (Error)FTSharp.FT_Outline_Get_Bitmap(library, outline, abitmap);

        internal static Error FT_Outline_Render(IntPtr library, IntPtr outline, IntPtr @params) =>
            (Error)FTSharp.FT_Outline_Render(library, outline, @params);

        internal static Orientation FT_Outline_Get_Orientation(IntPtr outline) =>
            (Orientation)FTSharp.FT_Outline_Get_Orientation(outline);

        #endregion

        #region Quick retrieval of advance values

        internal static Error FT_Get_Advance(IntPtr face, uint gIndex, LoadFlags load_flags, out IntPtr padvance) =>
            (Error)FTSharp.FT_Get_Advance(face, gIndex, (uint)load_flags, out padvance);

        internal static Error FT_Get_Advances(IntPtr face, uint start, uint count, LoadFlags load_flags, out IntPtr padvance) =>
            (Error)FTSharp.FT_Get_Advances(face, start, count, (uint)load_flags, out padvance);

        #endregion

        #region Bitmap Handling

        internal static void FT_Bitmap_New(IntPtr abitmap) =>
            FTSharp.FT_Bitmap_New(abitmap);

        internal static Error FT_Bitmap_Copy(IntPtr library, IntPtr source, IntPtr target) =>
            (Error)FTSharp.FT_Bitmap_Copy(library, source, target);

        internal static Error FT_Bitmap_Embolden(IntPtr library, IntPtr bitmap, IntPtr xStrength, IntPtr yStrength) =>
            (Error)FTSharp.FT_Bitmap_Embolden(library, bitmap, xStrength, yStrength);

        internal static Error FT_Bitmap_Convert(IntPtr library, IntPtr source, IntPtr target, int alignment) =>
            (Error)FTSharp.FT_Bitmap_Convert(library, source, target, alignment);

        internal static Error FT_GlyphSlot_Own_Bitmap(IntPtr slot) =>
            (Error)FTSharp.FT_GlyphSlot_Own_Bitmap(slot);

        internal static Error FT_Bitmap_Done(IntPtr library, IntPtr bitmap) =>
            (Error)FTSharp.FT_Bitmap_Done(library, bitmap);

        #endregion

        #region Glyph Stroker

        internal static StrokerBorder FT_Outline_GetInsideBorder(IntPtr outline) =>
            (StrokerBorder)FTSharp.FT_Outline_GetInsideBorder(outline);

        internal static StrokerBorder FT_Outline_GetOutsideBorder(IntPtr outline) =>
            (StrokerBorder)FTSharp.FT_Outline_GetOutsideBorder(outline);

        internal static Error FT_Stroker_New(IntPtr library, out IntPtr astroker) =>
            (Error)FTSharp.FT_Stroker_New(library, out astroker);

        internal static void FT_Stroker_Set(IntPtr stroker, int radius, StrokerLineCap line_cap, StrokerLineJoin line_join, IntPtr miter_limit) =>
            FTSharp.FT_Stroker_Set(stroker, radius, (FT_Stroker_LineCap)line_cap, (FT_Stroker_LineJoin)line_join, miter_limit);

        internal static void FT_Stroker_Rewind(IntPtr stroker) =>
            FTSharp.FT_Stroker_Rewind(stroker);

        internal static Error FT_Stroker_ParseOutline(IntPtr stroker, IntPtr outline, [MarshalAs(UnmanagedType.U1)] bool opened) =>
            (Error)FTSharp.FT_Stroker_ParseOutline(stroker, outline, opened);

        //internal static Error FT_Stroker_BeginSubPath(IntPtr stroker, ref FTVector to, [MarshalAs(UnmanagedType.U1)] bool open) =>
        //    (Error)FTSharp.FT_Stroker_BeginSubPath(stroker, ref to, open);

        //internal static Error FT_Stroker_EndSubPath(IntPtr stroker) =>
        //    (Error)FTSharp.FT_Stroker_EndSubPath(stroker);

        //internal static Error FT_Stroker_LineTo(IntPtr stroker, ref FTVector to) =>
        //    (Error)FTSharp.FT_Stroker_LineTo(stroker, ref to);

        //internal static Error FT_Stroker_ConicTo(IntPtr stroker, ref FTVector control, ref FTVector to) =>
        //    (Error)FTSharp.FT_Stroker_ConicTo(stroker, ref control, ref to);

        //internal static Error FT_Stroker_CubicTo(IntPtr stroker, ref FTVector control1, ref FTVector control2, ref FTVector to) =>
        //    (Error)FTSharp.FT_Stroker_CubicTo(stroker, ref control1, ref control2, ref to);

        internal static Error FT_Stroker_GetBorderCounts(IntPtr stroker, StrokerBorder border, out uint anum_points, out uint anum_contours) =>
            (Error)FTSharp.FT_Stroker_GetBorderCounts(stroker, (FT_StrokerBorder)border, out anum_points, out anum_contours);

        internal static void FT_Stroker_ExportBorder(IntPtr stroker, StrokerBorder border, IntPtr outline) =>
            FTSharp.FT_Stroker_ExportBorder(stroker, (FT_StrokerBorder)border, outline);

        internal static Error FT_Stroker_GetCounts(IntPtr stroker, out uint anum_points, out uint anum_contours) =>
            (Error)FTSharp.FT_Stroker_GetCounts(stroker, out anum_points, out anum_contours);

        internal static void FT_Stroker_Export(IntPtr stroker, IntPtr outline) =>
            FTSharp.FT_Stroker_Export(stroker, outline);

        internal static void FT_Stroker_Done(IntPtr stroker) =>
            FTSharp.FT_Stroker_Done(stroker);

        internal static Error FT_Glyph_Stroke(ref IntPtr pglyph, IntPtr stoker, [MarshalAs(UnmanagedType.U1)] bool destroy) =>
            (Error)FTSharp.FT_Glyph_Stroke(ref pglyph, stoker, destroy);

        internal static Error FT_Glyph_StrokeBorder(ref IntPtr pglyph, IntPtr stoker, [MarshalAs(UnmanagedType.U1)] bool inside, [MarshalAs(UnmanagedType.U1)] bool destroy) =>
            (Error)FTSharp.FT_Glyph_StrokeBorder(ref pglyph, stoker, inside, destroy);

        #endregion

        #region Module Management

        internal static Error FT_Add_Module(IntPtr library, IntPtr clazz) =>
            (Error)FTSharp.FT_Add_Module(library, clazz);

        internal static IntPtr FT_Get_Module(IntPtr library, string module_name) =>
            (IntPtr)FTSharp.FT_Get_Module(library, module_name);

        internal static Error FT_Remove_Module(IntPtr library, IntPtr module) =>
            (Error)FTSharp.FT_Remove_Module(library, module);

        internal static Error FT_Property_Set(IntPtr library, string module_name, string property_name, IntPtr value) =>
            (Error)FTSharp.FT_Property_Set(library, module_name, property_name, value);

        internal static Error FT_Property_Get(IntPtr library, string module_name, string property_name, IntPtr value) =>
            (Error)FTSharp.FT_Property_Get(library, module_name, property_name, value);

        internal static Error FT_Reference_Library(IntPtr library) =>
            (Error)FTSharp.FT_Reference_Library(library);

        internal static Error FT_New_Library(IntPtr memory, out IntPtr alibrary) =>
            (Error)FTSharp.FT_New_Library(memory, out alibrary);

        internal static Error FT_Done_Library(IntPtr library) =>
            (Error)FTSharp.FT_Done_Library(library);

        //TODO figure out the method signature for debug_hook. (FT_DebugHook_Func)
        internal static void FT_Set_Debug_Hook(IntPtr library, uint hook_index, IntPtr debug_hook) =>
            FTSharp.FT_Set_Debug_Hook(library, hook_index, debug_hook);

        internal static void FT_Add_Default_Modules(IntPtr library) =>
            FTSharp.FT_Add_Default_Modules(library);

        internal static IntPtr FT_Get_Renderer(IntPtr library, GlyphFormat format) =>
            (IntPtr)FTSharp.FT_Get_Renderer(library, (FT_Glyph_Format)format);

        internal static Error FT_Set_Renderer(IntPtr library, IntPtr renderer, uint num_params, IntPtr parameters) =>
            (Error)FTSharp.FT_Set_Renderer(library, renderer, num_params, parameters);

        #endregion

        #region GZIP Streams

        //internal static Error FT_Stream_OpenGzip(IntPtr stream, IntPtr source) =>
        //    (Error)FTSharp.FT_Stream_OpenGzip(stream);

        //internal static Error FT_Gzip_Uncompress(IntPtr memory, IntPtr output, ref IntPtr output_len, IntPtr input, IntPtr input_len) =>
        //    (Error)FTSharp.FT_Gzip_Uncompress(memory);

        #endregion

        #region LZW Streams

        //internal static Error FT_Stream_OpenLZW(IntPtr stream, IntPtr source) =>
        //    (Error)FTSharp.FT_Stream_OpenLZW(stream);

        #endregion

        #region BZIP2 Streams

        //internal static Error FT_Stream_OpenBzip2(IntPtr stream, IntPtr source) =>
        //    (Error)FTSharp.FT_Stream_OpenBzip2(stream);

        #endregion

        #region LCD Filtering

        //internal static Error FT_Library_SetLcdFilter(IntPtr library, LcdFilter filter) =>
        //    (Error)FTSharp.FT_Library_SetLcdFilter(library);

        //internal static Error FT_Library_SetLcdFilterWeights(IntPtr library, byte[] weights) =>
        //    (Error)FTSharp.FT_Library_SetLcdFilterWeights(library);

        #endregion

        #endregion

        #region Caching Sub-system

        //internal static Error FTC_Manager_New(IntPtr library, uint max_faces, uint max_sizes, ulong maxBytes, FaceRequester requester, IntPtr req_data, out IntPtr amanager) =>
        //    (Error)FTSharp.FTC_Manager_New(library, max_faces, max_sizes, maxBytes, requester, req_data, out amanager);

        //internal static void FTC_Manager_Reset(IntPtr manager) =>
        //    FTSharp.FTC_Manager_Reset(manager);

        //internal static void FTC_Manager_Done(IntPtr manager) =>
        //    FTSharp.FTC_Manager_Done(manager);

        //internal static Error FTC_Manager_LookupFace(IntPtr manager, IntPtr face_id, out IntPtr aface) =>
        //    (Error)FTSharp.FTC_Manager_LookupFace(manager, face_id, out aface);

        //internal static Error FTC_Manager_LookupSize(IntPtr manager, IntPtr scaler, out IntPtr asize) =>
        //    (Error)FTSharp.FTC_Manager_LookupSize(manager, scaler, out asize);

        //internal static void FTC_Node_Unref(IntPtr node, IntPtr manager) =>
        //    FTSharp.FTC_Node_Unref(node, manager);

        //internal static void FTC_Manager_RemoveFaceID(IntPtr manager, IntPtr face_id) =>
        //    FTSharp.FTC_Manager_RemoveFaceID(manager, face_id);

        //internal static Error FTC_CMapCache_New(IntPtr manager, out IntPtr acache) =>
        //    (Error)FTSharp.FTC_CMapCache_New(manager, out acache);

        //internal static uint FTC_CMapCache_Lookup(IntPtr cache, IntPtr face_id, int cmap_index, uint char_code) =>
        //    (uint)FTSharp.FTC_CMapCache_Lookup(cache, face_id, cmap_index, char_code);

        //internal static Error FTC_ImageCache_New(IntPtr manager, out IntPtr acache) =>
        //    (Error)FTSharp.FTC_ImageCache_New(manager, out acache);

        //internal static Error FTC_ImageCache_Lookup(IntPtr cache, IntPtr type, uint gindex, out IntPtr aglyph, out IntPtr anode) =>
        //    (Error)FTSharp.FTC_ImageCache_Lookup(cache, type, gindex, out aglyph, out anode);

        //internal static Error FTC_ImageCache_LookupScaler(IntPtr cache, IntPtr scaler, LoadFlags load_flags, uint gindex, out IntPtr aglyph, out IntPtr anode) =>
        //    (Error)FTSharp.FTC_ImageCache_LookupScaler(cache, scaler, (uint)load_flags, gindex, out aglyph, out anode);

        //internal static Error FTC_SBitCache_New(IntPtr manager, out IntPtr acache) =>
        //    (Error)FTSharp.FTC_SBitCache_New(manager, out acache);

        //internal static Error FTC_SBitCache_Lookup(IntPtr cache, IntPtr type, uint gindex, out IntPtr sbit, out IntPtr anode) =>
        //    (Error)FTSharp.FTC_SBitCache_Lookup(cache, type, gindex, out sbit, out anode);

        //internal static Error FTC_SBitCache_LookupScaler(IntPtr cache, IntPtr scaler, LoadFlags load_flags, uint gindex, out IntPtr sbit, out IntPtr anode) =>
        //    (Error)FTSharp.FTC_SBitCache_LookupScaler(cache, scaler, (uint)load_flags, gindex, out sbit, out anode);

        #endregion

        #region Miscellaneous

        #region OpenType Validation

        internal static Error FT_OpenType_Validate(IntPtr face, OpenTypeValidationFlags validation_flags, out IntPtr base_table, out IntPtr gdef_table, out IntPtr gpos_table, out IntPtr gsub_table, out IntPtr jsft_table) =>
            (Error)FTSharp.FT_OpenType_Validate(face, (uint)validation_flags, out base_table, out gdef_table, out gpos_table, out gsub_table, out jsft_table);

        internal static void FT_OpenType_Free(IntPtr face, IntPtr table) =>
            FTSharp.FT_OpenType_Free(face, table);

        #endregion

        #region The TrueType Engine

        internal static EngineType FT_Get_TrueType_Engine_Type(IntPtr library) =>
            (EngineType)FTSharp.FT_Get_TrueType_Engine_Type(library);

        #endregion

        #region TrueTypeGX/AAT Validation

        internal static Error FT_TrueTypeGX_Validate(IntPtr face, TrueTypeValidationFlags validation_flags, byte[][] tables, uint tableLength) =>
            (Error)FTSharp.FT_TrueTypeGX_Validate(face, (uint)validation_flags, ref tables[0], tableLength);

        internal static Error FT_TrueTypeGX_Free(IntPtr face, IntPtr table) =>
            (Error)FTSharp.FT_TrueTypeGX_Free(face, table);

        internal static Error FT_ClassicKern_Validate(IntPtr face, ClassicKernValidationFlags validation_flags, out IntPtr ckern_table) =>
            (Error)FTSharp.FT_ClassicKern_Validate(face, (uint)validation_flags, out ckern_table);

        internal static Error FT_ClassicKern_Free(IntPtr face, IntPtr table) =>
            (Error)FTSharp.FT_ClassicKern_Free(face, table);

        #endregion

        #endregion
    }
}
