<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" doctype-system="about:legacy-compat" encoding="UTF-8" indent="yes" />
  <xsl:variable name="newline" select="'&#10;'"/>
  <xsl:variable name="indent"><xsl:value-of select="$newline"/>   - </xsl:variable>
  <xsl:template match="Title" />
  <xsl:template match="TestResult"><xsl:value-of select="Code" />: <xsl:value-of select="Title" /> : <xsl:value-of select="Outcome" /><xsl:apply-templates select="Exception" /></xsl:template>
  <xsl:template match="Exception"><xsl:apply-templates select="Message" /></xsl:template>
  <xsl:template match="Message"><xsl:if test="normalize-space(child::text()) != ''"><xsl:value-of select="concat($indent,normalize-space(child::text()))" /></xsl:if>
    <xsl:apply-templates select="Stack" /><xsl:apply-templates select="*/Message" /></xsl:template>
  <xsl:template match="Stack"><xsl:value-of select="$indent"/>OperationOutcome.Issue(<xsl:value-of select="attribute::issue" />): <xsl:value-of select="child::text()" />
  </xsl:template>
</xsl:stylesheet>