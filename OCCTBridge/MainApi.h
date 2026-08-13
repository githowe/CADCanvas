#pragma once

#include "Wrapper.h"

/// <summary>
/// 初始化二维点缓存
/// </summary>
dll_export void InitPoint2DCache(double* cache, int size);

/// <summary>
/// 创建直线
/// </summary>
dll_export void* CreateLine(double x1, double y1, double x2, double y2);

/// <summary>
/// 创建直线段
/// </summary>
dll_export void* CreateLineSegment(double x1, double y1, double x2, double y2);

/// <summary>
/// 创建圆形
/// </summary>
dll_export void* CreateCircle(double centerX, double centerY, double radius);

/// <summary>
/// 设置直线段起点
/// </summary>
dll_export void SetLineSegmentStart(LineSegmentWrapper* wrapper, double x, double y);

/// <summary>
/// 设置直线段终点
/// </summary>
dll_export void SetLineSegmentEnd(LineSegmentWrapper* wrapper, double x, double y);

/// <summary>
/// 释放曲线
/// </summary>
dll_export void FreeCurve(CurveWrapper* curve);

/// <summary>
/// 获取两条曲线的交点
/// </summary>
dll_export int GetIntersection(CurveWrapper* curve1, CurveWrapper* curve2);

/// <summary>
/// 获取曲线与射线的交点
/// </summary>
dll_export int GetIntersectionWithRay(CurveWrapper* curve, double x, double y, double dx, double dy);

/// <summary>
/// 判断两个曲线是否相交
/// </summary>
dll_export int IsIntersection(CurveWrapper* curve1, CurveWrapper* curve2);

/// <summary>
/// 判断曲线是否与矩形相交
/// </summary>
dll_export int IsIntersectionWithRect(CurveWrapper* curve, double left, double top, double right, double bottom);
