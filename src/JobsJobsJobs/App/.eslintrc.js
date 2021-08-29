module.exports = {
  root: true,
  env: {
    node: true
  },
  extends: [
    "plugin:vue/vue3-essential",
    "@vue/standard",
    "@vue/typescript/recommended"
  ],
  parserOptions: {
    ecmaVersion: 2020
  },
  globals: {
    defineProps: "readonly",
    defineEmits: "readonly",
    defineExpose: "readonly",
    withDefaults: "readonly"
  },
  rules: {
    "no-console": process.env.NODE_ENV === "production" ? "warn" : "off",
    "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
    "vue/no-multiple-template-root": "off",
    "vue/script-setup-uses-vars": 1,
    "quotes": ["error", "double", { avoidEscape: true, allowTemplateLiterals: true }],
    "func-call-spacing": "off",
    "@typescript-eslint/no-unused-vars": "off"
  }
}
