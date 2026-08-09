#include "MainApi.h"
#include "GlobalCache.h"

#include <Geom2dAPI_InterCurveCurve.hxx>

void InitPoint2DCache(double* cache, int size)
{
	GlobalCache::GetInstance().Point2DCache = cache;
	GlobalCache::GetInstance().Point2DCacheSize = size;
}

void* CreateLine(double x1, double y1, double x2, double y2)
{
	LineWrapper* wrapper = new LineWrapper();
	wrapper->Line = new Geom2d_Line(gp_Pnt2d(x1, y1), gp_Dir2d(x2 - x1, y2 - y1));
	return wrapper;
}

void* CreateLineSegment(double x1, double y1, double x2, double y2)
{
	LineSegmentWrapper* wrapper = new LineSegmentWrapper();
	wrapper->LineSegment = GC_MakeSegment2d(gp_Pnt2d(x1, y1), gp_Pnt2d(x2, y2));

	return wrapper;
}

void* CreateCircle(double centerX, double centerY, double radius)
{
	CircleWrapper* wrapper = new CircleWrapper();
	wrapper->Circle = new Geom2d_Circle(gp_Ax2d(gp_Pnt2d(centerX, centerY), gp_Dir2d(1, 0)), radius);
	return wrapper;
}

void SetLineSegmentStart(LineSegmentWrapper* wrapper, double x, double y)
{
	// 获取终点
	gp_Pnt2d endPoint = wrapper->LineSegment->EndPoint();
	// 创建新的直线段
	wrapper->LineSegment = GC_MakeSegment2d(gp_Pnt2d(x, y), endPoint);
}

void SetLineSegmentEnd(LineSegmentWrapper* wrapper, double x, double y)
{
	// 获取起点
	gp_Pnt2d startPoint = wrapper->LineSegment->StartPoint();
	// 创建新的直线段
	wrapper->LineSegment = GC_MakeSegment2d(startPoint, gp_Pnt2d(x, y));
}

void FreeCurve(CurveWrapper* curve)
{
	// 释放曲线内对象
	curve->FreeCurve();
	// 释放自身
	delete curve;
}

int GetIntersection(CurveWrapper* curve1, CurveWrapper* curve2)
{
	Geom2dAPI_InterCurveCurve intersector(curve1->GetCurve(), curve2->GetCurve());
	int pointCount = intersector.NbPoints();

	// 如果交点数量超过缓存大小，返回失败
	if (pointCount > GlobalCache::GetInstance().Point2DCacheSize / 2) return -1;
	// 遍历交点并将其存储到全局缓存中
	if (pointCount > 0)
	{
		for (int index = 1; index <= pointCount; ++index)
		{
			gp_Pnt2d intersectionPoint = intersector.Point(index);
			GlobalCache::GetInstance().Point2DCache[(index - 1) * 2] = intersectionPoint.X();
			GlobalCache::GetInstance().Point2DCache[(index - 1) * 2 + 1] = intersectionPoint.Y();
		}
	}
	// 返回交点数量
	return pointCount;
}

int GetIntersectionWithRay(CurveWrapper* curve, double x, double y, double dx, double dy)
{
	// 创建直线
	Handle(Geom2d_Curve) line = new Geom2d_Line(gp_Pnt2d(x, y), gp_Dir2d(dx, dy));
	// 获取与曲线的交点
	Geom2dAPI_InterCurveCurve intersector(curve->GetCurve(), line);
	int pointCount = intersector.NbPoints();
	// 创建点数组
	std::vector<gp_Pnt2d> points;
	// 创建原点和方向向量
	gp_Pnt2d origin(x, y);
	gp_Vec2d directionVector(dx, dy);
	// 遍历全部交点，过滤掉射线反方向的交点
	for (int index = 1; index <= pointCount; ++index)
	{
		// 创建从原点指向交点的向量
		gp_Pnt2d intersectionPoint = intersector.Point(index);
		gp_Vec2d pointVector(origin, intersectionPoint);
		// 如果点向量与方向向量的点积大于等于0，则说明交点在射线方向上
		if (directionVector.Dot(pointVector) >= 0)
			points.push_back(intersectionPoint);
	}
	pointCount = points.size();

	// 如果交点数量超过缓存大小，返回失败
	if (pointCount > GlobalCache::GetInstance().Point2DCacheSize / 2) return -1;
	// 遍历交点并将其存储到全局缓存中
	if (pointCount > 0)
	{
		for (int index = 0; index < pointCount; ++index)
		{
			gp_Pnt2d intersectionPoint = points[index];
			GlobalCache::GetInstance().Point2DCache[index * 2] = intersectionPoint.X();
			GlobalCache::GetInstance().Point2DCache[index * 2 + 1] = intersectionPoint.Y();
		}
	}
	// 返回交点数量
	return pointCount;
}
