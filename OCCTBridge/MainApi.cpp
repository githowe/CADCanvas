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
