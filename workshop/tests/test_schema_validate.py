import unittest

from mcrimworld_workshop.schema_validate import validate


class SchemaValidateTests(unittest.TestCase):
    def test_simple_object_passes(self):
        schema = {
            "type": "object",
            "additionalProperties": False,
            "required": ["name"],
            "properties": {"name": {"type": "string"}, "age": {"type": "integer"}},
        }
        result = validate({"name": "Joe", "age": 5}, schema)
        self.assertTrue(result.valid, result.errors)

    def test_missing_required_fails(self):
        schema = {"type": "object", "required": ["name"], "properties": {"name": {"type": "string"}}}
        result = validate({}, schema)
        self.assertFalse(result.valid)
        self.assertIn("missing required property 'name'", result.errors[0])

    def test_additional_property_rejected(self):
        schema = {"type": "object", "additionalProperties": False, "properties": {"a": {"type": "string"}}}
        result = validate({"a": "x", "b": "y"}, schema)
        self.assertFalse(result.valid)

    def test_enum_rejects_unknown_value(self):
        schema = {"type": "string", "enum": ["a", "b"]}
        self.assertFalse(validate("c", schema).valid)
        self.assertTrue(validate("a", schema).valid)

    def test_ref_resolution(self):
        schema = {
            "$defs": {"status": {"type": "string", "enum": ["ok", "bad"]}},
            "type": "object",
            "properties": {"status": {"$ref": "#/$defs/status"}},
        }
        self.assertTrue(validate({"status": "ok"}, schema).valid)
        self.assertFalse(validate({"status": "nope"}, schema).valid)

    def test_array_items_and_unique(self):
        schema = {"type": "array", "items": {"type": "string"}, "uniqueItems": True, "minItems": 1}
        self.assertTrue(validate(["a", "b"], schema).valid)
        self.assertFalse(validate(["a", "a"], schema).valid)
        self.assertFalse(validate([], schema).valid)

    def test_wrong_type_reported(self):
        result = validate("not an object", {"type": "object"})
        self.assertFalse(result.valid)
        self.assertIn("expected object", result.errors[0])


if __name__ == "__main__":
    unittest.main()
