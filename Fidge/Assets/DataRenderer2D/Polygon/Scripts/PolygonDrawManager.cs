using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geniikw.DataRenderer2D.Polygon
{
    public class PolygonDrawerManager 
    {
        IPolygon _target;
        IMeshDrawer _normal;
        IMeshDrawer _hole;

        public PolygonDrawerManager(IPolygon target ,
            IMeshDrawer normal, IMeshDrawer hole)
        {
            _target = target;
            _normal = normal;
            _hole = hole;
        }

        public IEnumerable<IMesh> Draw()
        {
            var polyGon = _target.Polygon;

            if (polyGon.count < 3)
                yield break;
            
            if (polyGon.type == PolygonType.ZigZag)
                foreach (var m in _normal.Draw())
                    yield return m;

            else if (polyGon.type >= PolygonType.Hole)
                foreach (var m in _hole.Draw())
                    yield return m;
        }
        
    }
}