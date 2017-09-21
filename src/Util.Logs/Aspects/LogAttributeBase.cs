﻿using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Util.Logs.Abstractions;
using Util.Logs.Extensions;

namespace Util.Logs.Aspects {
    /// <summary>
    /// 日志操作
    /// </summary>
    public abstract class LogAttributeBase : AbstractInterceptorAttribute {
        /// <summary>
        /// 执行
        /// </summary>
        public override async Task Invoke( AspectContext context, AspectDelegate next ) {
            var methodName = GetMethodName( context );
            var manager = (ILogManager)context.ServiceProvider.GetService( typeof( ILogManager ) );
            var log = manager.GetLog( methodName );
            if( !Enabled( log ) )
                return;
            ExecuteBefore( log, context, methodName );
            await next( context );
            ExecuteAfter( log, context, methodName );
        }

        /// <summary>
        /// 获取方法名
        /// </summary>
        private string GetMethodName( AspectContext context ) {
            return $"{context.ServiceMethod.DeclaringType.FullName}.{context.ServiceMethod.Name}";
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        protected virtual bool Enabled( ILog log ) {
            return true;
        }

        /// <summary>
        /// 执行前
        /// </summary>
        private void ExecuteBefore( ILog log, AspectContext context, string methodName ) {
            log.Caption( $"{context.ServiceMethod.Name}方法执行前" )
                .Method( methodName );
            foreach( var parameter in context.GetParameters() )
                parameter.AppendTo( log );
            WriteLog( log );
        }

        /// <summary>
        /// 写日志
        /// </summary>
        protected abstract void WriteLog( ILog log );

        /// <summary>
        /// 执行后
        /// </summary>
        private void ExecuteAfter( ILog log, AspectContext context, string methodName ) {
            var parameter = context.GetReturnParameter();
            log.Caption( $"{context.ServiceMethod.Name}方法执行后" )
                .Method( methodName )
                .Content( "返回类型: {0},返回值: {1}", parameter.ParameterInfo.ParameterType.FullName, parameter.Value.SafeString() );
            WriteLog( log );
        }
    }
}
