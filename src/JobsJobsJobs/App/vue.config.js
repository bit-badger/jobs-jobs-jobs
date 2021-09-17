module.exports = {
  transpileDependencies: [
    'vuetify'
  ],
  outputDir: '../Api/wwwroot',
  configureWebpack: {
    module: {
      rules: [{
        test: /\.mjs$/,
        include: /node_modules/,
        type: "javascript/auto"
      }]
    }
  }
}
