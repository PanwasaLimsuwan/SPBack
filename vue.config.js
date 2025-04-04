module.exports = {
  lintOnSave: false,
  transpileDependencies: [
    'es5-ext',
  ],
  configureWebpack: {
    resolve: {
      fallback: {
        stream: require.resolve('stream-browserify'),
        assert: require.resolve('assert'),
      },
    },
  },
  devServer: {
    proxy: 'http://localhost:5000', // กำหนดให้ Proxy ไปที่ API ที่กำลังรันอยู่ที่ localhost:5000
  },
}
